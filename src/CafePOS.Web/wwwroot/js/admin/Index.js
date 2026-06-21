// Admin Module SPA Controller
const API_URL = "http://localhost:5076/api/v1";

// Global datasets for search/pagination
let categoriesData = [];
let categorySearchQuery = "";
let categoryFilterActiveOnly = false;
let categoryCurrentPage = 1;
const categoriesPerPage = 5;

let productsData = [];
let productSearchQuery = "";
let toppingsData = [];
let toppingSearchQuery = "";

// DOM Elements
let salesChartInstance = null;

// Immediately check auth status
(function checkAuth() {
    const token = localStorage.getItem("token");
    const staffStr = localStorage.getItem("staff");
    const loginMode = localStorage.getItem("loginMode");

    if (!token || !staffStr || loginMode !== "Admin") {
        window.location.href = "/Admin/Login";
        return;
    }

    try {
        const staff = JSON.parse(staffStr);
        if (staff.role !== "Owner" && staff.role !== "ShiftLeader") {
            alert("Bạn không có quyền truy cập trang quản trị!");
            window.location.href = "/";
        }
    } catch (e) {
        localStorage.clear();
        window.location.href = "/Admin/Login";
    }
})();

// Document Ready
document.addEventListener("DOMContentLoaded", () => {
    // Populate header info
    populateAdminProfile();

    // Setup navigation tab clicks
    setupNavigation();

    // Default load Dashboard view
    switchTab("dashboard");
});

// Populate Profile Information
function populateAdminProfile() {
    const staffStr = localStorage.getItem("staff");
    if (staffStr) {
        try {
            const staff = JSON.parse(staffStr);
            const friendlyRole = translateRole(staff.role);
            
            // Header bar
            document.getElementById("admin-name").innerText = staff.fullName;
            document.getElementById("admin-role").innerText = friendlyRole;
            
            // Dropdown menu
            document.getElementById("dd-admin-name").innerText = staff.fullName;
            document.getElementById("dd-admin-role").innerText = friendlyRole;
        } catch(e) {}
    }
}

// Translate Role helper
function translateRole(role) {
    switch (role) {
        case "Owner": return "Chủ cửa hàng";
        case "ShiftLeader": return "Trưởng ca";
        case "Cashier": return "Thu ngân";
        case "Barista": return "Pha chế";
        case "PastryStaff": return "Quầy bánh";
        default: return role;
    }
}

// Switch Side Menu tabs (SPA style)
function setupNavigation() {
    const menuItems = document.querySelectorAll(".sidebar-menu .menu-item, .submenu-item");
    
    menuItems.forEach(item => {
        item.addEventListener("click", (e) => {
            const tabId = item.getAttribute("data-tab");
            if (tabId) {
                e.preventDefault();
                switchTab(tabId);
                
                // Remove active class from all
                menuItems.forEach(mi => mi.classList.remove("active"));
                
                // Add active to current
                item.classList.add("active");
                
                // If it's a submenu item, also set parent trigger to active
                const parentGroup = item.closest(".menu-group");
                if (parentGroup) {
                    const trigger = parentGroup.querySelector(".collapsed-trigger");
                    if (trigger) trigger.classList.add("active");
                }
            }
        });
    });
}

function toggleMenuCollapse(elementId) {
    const el = document.getElementById(elementId);
    if (el) {
        const isCollapsed = el.classList.contains("show");
        if (isCollapsed) {
            el.classList.remove("show");
        } else {
            el.classList.add("show");
        }
    }
}

// Router/Tab Switcher
function switchTab(tabId) {
    // Hide all views
    document.querySelectorAll(".admin-view-panel").forEach(panel => {
        panel.classList.remove("active");
    });
    
    // Show destination view
    const targetPanel = document.getElementById(`view-${tabId}`);
    if (targetPanel) {
        targetPanel.classList.add("active");
    }

    // Set Context Title
    const headerTitle = document.getElementById("header-context-title");
    headerTitle.innerText = getTabTitle(tabId);

    // Call individual loaders
    switch (tabId) {
        case "dashboard":
            loadDashboardStats();
            break;
        case "categories":
            loadCategories();
            break;
        case "products":
            loadProducts();
            break;
        case "staff":
            loadStaffList();
            break;
        case "vouchers":
            loadVouchersList();
            break;
        case "inventory":
            loadInventoryList();
            break;
        case "toppings":
            loadToppings();
            break;
    }
}

// Tab titles mappings
function getTabTitle(tabId) {
    switch (tabId) {
        case "dashboard": return "Dashboard Overview";
        case "categories": return "Quản lý thực đơn > Danh mục";
        case "products": return "Quản lý thực đơn > Sản phẩm";
        case "toppings": return "Quản lý thực đơn > Topping";
        case "staff": return "Quản lý Nhân viên";
        case "vouchers": return "Vouchers & Khuyến mãi";
        case "inventory": return "Kho nguyên liệu";
        case "shifts": return "Ca làm việc";
        case "reports": return "Báo cáo doanh thu";
        case "settings": return "Cài đặt hệ thống";
        default: return "CafePOS Admin";
    }
}

// Toast Alert notification helper
function showToast(msg, type = "success") {
    // Check if container exists
    let toastContainer = document.querySelector(".toast-container");
    if (!toastContainer) {
        toastContainer = document.createElement("div");
        toastContainer.className = "toast-container";
        document.body.appendChild(toastContainer);
    }
    
    const toast = document.createElement("div");
    toast.className = `toast show align-items-center border-0 mb-2 admin-toast`;
    toast.style.borderLeftColor = type === "success" ? "#35571b" : "#ba1a1a";
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body d-flex align-items-center gap-2">
                <span class="material-symbols-outlined text-${type === "success" ? "success" : "danger"}">
                    ${type === "success" ? "check_circle" : "error"}
                </span>
                <span class="fw-semibold text-dark">${msg}</span>
            </div>
            <button type="button" class="btn-close btn-close-dark me-2 m-auto" data-bs-dismiss="toast" aria-label="Close" onclick="this.closest('.toast').remove()"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    
    // Auto remove after 3s
    setTimeout(() => {
        toast.remove();
    }, 3000);
}

// Toast actions
function showNewOrderToast() {
    showToast("Tính năng tạo đơn từ Admin đang phát triển.", "warning");
}
function showNotificationToast() {
    showToast("Bạn không có thông báo mới nào.", "success");
}
function showHelpModal() {
    const modal = new bootstrap.Modal(document.getElementById('helpModal'));
    modal.show();
}

// ----------------------------------------------------
// 1. DASHBOARD OVERVIEW LOAD
// ----------------------------------------------------
async function loadDashboardStats() {
    const token = localStorage.getItem("token");
    
    try {
        const res = await fetch(`${API_URL}/reports/dashboard-stats`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await res.json();
        
        if (res.ok && result.success) {
            const stats = result.data;
            
            // Set numeric stats
            document.getElementById("stat-revenue").innerText = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(stats.totalRevenue);
            document.getElementById("stat-orders").innerText = stats.totalOrders;
            document.getElementById("stat-staff").innerText = stats.activeStaffCount;
            document.getElementById("stat-stock-alert").innerText = stats.lowStockIngredientsCount;
            
            // Build recent orders table
            renderDashboardRecentOrders(stats.recentOrders);
            
            // Build sales chart
            renderSalesChart(stats.weeklyRevenue);

            // Fetch low stock items list for side panel
            loadDashboardLowStockList();
        }
    } catch (err) {
        console.error("Error loading dashboard stats", err);
    }
}

function renderDashboardRecentOrders(orders) {
    const tbody = document.getElementById("dashboard-recent-orders");
    if (!orders || orders.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center py-3 text-muted">Không có đơn hàng nào gần đây.</td></tr>`;
        return;
    }
    
    tbody.innerHTML = orders.map(o => {
        const formattedDate = new Date(o.createdAt).toLocaleDateString("vi-VN", { hour: '2-digit', minute: '2-digit' });
        const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(o.totalAmount);
        
        let statusBadge = "bg-secondary-light-badge";
        let statusText = o.status;
        if (o.status === "Completed" || o.status === "Closed") {
            statusBadge = "bg-success-light-badge";
            statusText = "Thành công";
        } else if (o.status === "Cancelled") {
            statusBadge = "bg-danger-light-badge";
            statusText = "Đã huỷ";
        } else {
            statusBadge = "bg-warning-light-badge";
            statusText = "Đang xử lý";
        }
        
        return `
            <tr>
                <td class="fw-bold">${o.orderCode}</td>
                <td>${o.customerName}</td>
                <td>${formattedDate}</td>
                <td class="fw-bold text-success">${price}</td>
                <td><span class="badge ${statusBadge}">${statusText}</span></td>
            </tr>
        `;
    }).join('');
}

function renderSalesChart(weeklyRevenue) {
    const ctx = document.getElementById('salesChart').getContext('2d');
    
    const labels = weeklyRevenue.map(d => `${d.date} (${d.dayOfWeek})`);
    const data = weeklyRevenue.map(d => d.revenue);
    
    if (salesChartInstance) {
        salesChartInstance.destroy();
    }
    
    salesChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu hàng ngày (VND)',
                data: data,
                backgroundColor: 'rgba(76, 112, 49, 0.75)', // matcha-primary
                borderColor: '#35571b',
                borderWidth: 1.5,
                borderRadius: 8,
                barPercentage: 0.6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return (value / 1000) + 'k';
                        }
                    },
                    grid: {
                        color: 'rgba(0,0,0,0.03)'
                    }
                },
                x: {
                    grid: { display: false }
                }
            }
        }
    });
}

async function loadDashboardLowStockList() {
    const listEl = document.getElementById("dashboard-low-stock-list");
    const token = localStorage.getItem("token");
    
    try {
        const res = await fetch(`${API_URL}/inventory`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        // We know inventory is a placeholder, so if it fails, we fall back to a mock list
        let ingredients = [];
        
        if (res.ok) {
            const result = await res.json();
            ingredients = result.data || [];
        } else {
            // Mock data representing database seeded ingredients
            ingredients = [
                { name: "Bột trà sữa", currentQuantity: 1.5, minAlertQuantity: 2.0, unit: "kg" },
                { name: "Bột Matcha Uji", currentQuantity: 0.3, minAlertQuantity: 0.5, unit: "kg" },
                { name: "Hạt cà phê Robusta", currentQuantity: 8.0, minAlertQuantity: 3.0, unit: "kg" },
                { name: "Sữa đặc", currentQuantity: 4.0, minAlertQuantity: 6.0, unit: "lon" }
            ];
        }
        
        const lowStock = ingredients.filter(i => i.currentQuantity <= i.minAlertQuantity);
        
        if (lowStock.length === 0) {
            listEl.innerHTML = `
                <div class="text-center py-4 text-success small">
                    <span class="material-symbols-outlined text-[32px] d-block mb-1">check_circle</span>
                    Kho đạt ngưỡng an toàn! Không có cảnh báo.
                </div>
            `;
            return;
        }
        
        listEl.innerHTML = lowStock.map(i => `
            <div class="list-group-item d-flex justify-content-between align-items-center border-0 px-0 py-2">
                <div>
                    <strong class="text-dark d-block small">${i.name}</strong>
                    <span class="text-danger small">Tồn kho thấp: ${i.currentQuantity} / ${i.minAlertQuantity} ${i.unit}</span>
                </div>
                <span class="badge bg-danger-light-badge">Cần nhập hàng</span>
            </div>
        `).join('');
    } catch(err) {
        listEl.innerHTML = `<div class="text-center py-3 text-danger small">Lỗi tải dữ liệu kho</div>`;
    }
}


// ----------------------------------------------------
// 2. CATEGORY MANAGEMENT LOAD & CRUD (Matches Screenshot)
// ----------------------------------------------------
async function loadCategories() {
    const tbody = document.getElementById("categories-table-body");
    const token = localStorage.getItem("token");
    
    try {
        const res = await fetch(`${API_URL}/products/categories`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await res.json();
        
        if (res.ok && result.success) {
            categoriesData = result.data;
            renderCategoriesTable();
        } else {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-danger">Không thể tải danh sách danh mục.</td></tr>`;
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-danger">Lỗi kết nối máy chủ API.</td></tr>`;
    }
}

function renderCategoriesTable() {
    const tbody = document.getElementById("categories-table-body");
    
    // Apply client filters
    let filtered = categoriesData.filter(c => {
        const matchesSearch = c.name.toLowerCase().includes(categorySearchQuery.toLowerCase());
        const matchesActive = !categoryFilterActiveOnly || c.isActive;
        return matchesSearch && matchesActive;
    });
    
    // Pagination slicing
    const totalCount = filtered.length;
    const totalPages = Math.ceil(totalCount / categoriesPerPage) || 1;
    if (categoryCurrentPage > totalPages) categoryCurrentPage = totalPages;
    
    const startIdx = (categoryCurrentPage - 1) * categoriesPerPage;
    const paginated = filtered.slice(startIdx, startIdx + categoriesPerPage);
    
    // Update pagination text
    const displayStart = totalCount === 0 ? 0 : startIdx + 1;
    const displayEnd = Math.min(startIdx + categoriesPerPage, totalCount);
    document.getElementById("category-pagination-info").innerText = 
        `Hiển thị ${displayStart}-${displayEnd} trên tổng số ${totalCount} danh mục`;
        
    // Render pagination controls
    renderCategoryPaginationControls(totalPages);
    
    if (paginated.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center py-5 text-muted">Không tìm thấy danh mục nào phù hợp.</td></tr>`;
        return;
    }
    
    tbody.innerHTML = paginated.map(c => {
        const statusBadge = c.isActive ? "bg-success-light-badge" : "bg-secondary-light-badge";
        const statusText = c.isActive ? "Hoạt động" : "Ngừng bán";
        const friendlyStation = c.displayStation === "Bar" ? "Quầy Bar" : (c.displayStation === "Pastry" ? "Tủ Bánh" : "Cả hai");
        
        return `
            <tr>
                <td>
                    <input type="checkbox" class="form-check-input category-row-check" data-id="${c.id}">
                </td>
                <td>
                    <div class="d-flex align-items-center gap-3">
                        <div class="category-icon-box">
                            <span class="material-symbols-outlined">folder</span>
                        </div>
                        <span class="fw-bold text-dark">${c.name}</span>
                    </div>
                </td>
                <td><span class="badge bg-light text-dark border">${friendlyStation}</span></td>
                <td class="fw-bold">${c.productsCount}</td>
                <td><span class="badge ${statusBadge}">${statusText}</span></td>
                <td class="text-end">
                    <button class="btn btn-sm btn-link text-success p-0 me-3" onclick="openEditCategoryModal(${c.id})">Sửa</button>
                    <button class="btn btn-sm btn-link text-danger p-0 me-2" onclick="deleteCategory(${c.id})">Xóa</button>
                </td>
            </tr>
        `;
    }).join('');
}

function renderCategoryPaginationControls(totalPages) {
    const controls = document.getElementById("category-pagination-controls");
    let html = '';
    
    // Prev button
    html += `
        <li class="page-item ${categoryCurrentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="changeCategoryPage(${categoryCurrentPage - 1}); return false;">‹</a>
        </li>
    `;
    
    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        html += `
            <li class="page-item ${categoryCurrentPage === i ? 'active' : ''}">
                <a class="page-link" href="#" onclick="changeCategoryPage(${i}); return false;">${i}</a>
            </li>
        `;
    }
    
    // Next button
    html += `
        <li class="page-item ${categoryCurrentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="changeCategoryPage(${categoryCurrentPage + 1}); return false;">›</a>
        </li>
    `;
    
    controls.innerHTML = html;
}

function changeCategoryPage(page) {
    categoryCurrentPage = page;
    renderCategoriesTable();
}

function handleCategorySearch(val) {
    categorySearchQuery = val;
    categoryCurrentPage = 1;
    renderCategoriesTable();
}

function toggleCategoryActiveFilter() {
    categoryFilterActiveOnly = !categoryFilterActiveOnly;
    categoryCurrentPage = 1;
    renderCategoriesTable();
    showToast(categoryFilterActiveOnly ? "Đã lọc hiển thị danh mục hoạt động" : "Hiển thị tất cả danh mục");
}

function toggleSelectAllCategories(masterCheck) {
    const checkboxes = document.querySelectorAll(".category-row-check");
    checkboxes.forEach(cb => cb.checked = masterCheck.checked);
}

// Category CRUD Modal triggers
function openAddCategoryModal() {
    document.getElementById("categoryModalLabel").innerText = "Thêm danh mục mới";
    document.getElementById("category-id-input").value = "";
    document.getElementById("category-name-input").value = "";
    document.getElementById("category-station-input").value = "Bar";
    document.getElementById("category-active-input").checked = true;
    
    const modal = new bootstrap.Modal(document.getElementById('categoryModal'));
    modal.show();
}

function openEditCategoryModal(id) {
    const category = categoriesData.find(c => c.id === id);
    if (!category) return;
    
    document.getElementById("categoryModalLabel").innerText = "Chỉnh sửa danh mục";
    document.getElementById("category-id-input").value = category.id;
    document.getElementById("category-name-input").value = category.name;
    document.getElementById("category-station-input").value = category.displayStation;
    document.getElementById("category-active-input").checked = category.isActive;
    
    const modal = new bootstrap.Modal(document.getElementById('categoryModal'));
    modal.show();
}

// Save category submit
async function submitCategoryForm() {
    const id = document.getElementById("category-id-input").value;
    const name = document.getElementById("category-name-input").value.trim();
    const displayStation = document.getElementById("category-station-input").value;
    const isActive = document.getElementById("category-active-input").checked;
    const token = localStorage.getItem("token");

    if (!name) {
        alert("Vui lòng nhập tên danh mục!");
        return;
    }

    const payload = { name, displayStation, isActive };
    const method = id ? "PUT" : "POST";
    const endpoint = id ? `${API_URL}/products/categories/${id}` : `${API_URL}/products/categories`;

    try {
        const res = await fetch(endpoint, {
            method: method,
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });
        
        const result = await res.json();
        if (res.ok && result.success) {
            showToast(result.message || "Lưu danh mục thành công!");
            
            // Hide modal
            const modalEl = document.getElementById('categoryModal');
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            if (modalInstance) modalInstance.hide();
            
            // Reload list
            loadCategories();
        } else {
            alert(result.message || "Có lỗi xảy ra khi lưu danh mục.");
        }
    } catch (err) {
        console.error(err);
        alert("Lỗi kết nối máy chủ API.");
    }
}

// Delete category
async function deleteCategory(id) {
    const category = categoriesData.find(c => c.id === id);
    if (!category) return;

    if (category.productsCount > 0) {
        alert(`Không thể xóa danh mục '${category.name}' vì đang chứa ${category.productsCount} món ăn/đồ uống!`);
        return;
    }

    if (!confirm(`Bạn có chắc chắn muốn xóa danh mục '${category.name}' không?`)) {
        return;
    }

    const token = localStorage.getItem("token");

    try {
        const res = await fetch(`${API_URL}/products/categories/${id}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await res.json();
        
        if (res.ok && result.success) {
            showToast(result.message || "Đã xóa danh mục!");
            loadCategories();
        } else {
            alert(result.message || "Xóa danh mục thất bại.");
        }
    } catch (err) {
        console.error(err);
        alert("Lỗi kết nối máy chủ API.");
    }
}


// ----------------------------------------------------
// 3. PRODUCT MANAGEMENT VIEW
// ----------------------------------------------------
async function loadProducts() {
    const tbody = document.getElementById("products-table-body");
    const token = localStorage.getItem("token");
    
    try {
        const res = await fetch(`${API_URL}/products`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await res.json();
        
        if (res.ok && result.success) {
            productsData = result.data;
            renderProductsTable();
        } else {
            tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-danger">Không thể tải sản phẩm.</td></tr>`;
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-danger">Lỗi kết nối máy chủ API.</td></tr>`;
    }
}

function renderProductsTable() {
    const tbody = document.getElementById("products-table-body");
    
    let filtered = productsData.filter(p => {
        return p.name.toLowerCase().includes(productSearchQuery.toLowerCase()) || 
               p.categoryName.toLowerCase().includes(productSearchQuery.toLowerCase());
    });
    
    if (filtered.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center py-4 text-muted">Không tìm thấy sản phẩm nào.</td></tr>`;
        return;
    }
    
    tbody.innerHTML = filtered.map(p => {
        const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(p.basePrice);
        const statusBadge = p.status === "Active" ? "bg-success-light-badge" : "bg-danger-light-badge";
        const statusText = p.status === "Active" ? "Hoạt động" : (p.status === "OutOfStock" ? "Hết hàng" : "Ngừng bán");
        
        let sizesStr = "Mặc định";
        if (p.hasSizeOption && p.sizes && p.sizes.length > 0) {
            sizesStr = p.sizes.map(s => `${s.sizeLabel} (+${s.priceModifier / 1000}k)`).join(', ');
        }
        
        return `
            <tr>
                <td>
                    <img src="${p.imageUrl || 'https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=100&auto=format&fit=crop&q=60'}" alt="${p.name}" class="rounded" style="width:40px; height:40px; object-fit:cover; border:1px solid #ddd;" onerror="this.src='https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=100&auto=format&fit=crop&q=60'" />
                </td>
                <td>
                    <strong class="text-dark d-block">${p.name}</strong>
                    <span class="text-muted small">${p.description}</span>
                </td>
                <td><span class="badge bg-light text-dark border">${p.categoryName}</span></td>
                <td class="fw-bold text-success">${price}</td>
                <td><span class="small">${sizesStr}</span></td>
                <td><span class="badge ${statusBadge}">${statusText}</span></td>
                <td class="text-end">
                    <button class="btn btn-sm btn-link text-success p-0 me-3" onclick="openEditProductModal(${p.id})">Sửa</button>
                    <button class="btn btn-sm btn-link text-danger p-0 me-2" onclick="deleteProduct(${p.id})">Xóa</button>
                </td>
            </tr>
        `;
    }).join('');
}

function handleProductSearch(val) {
    productSearchQuery = val;
    renderProductsTable();
}


// ----------------------------------------------------
// 4. MOCK DATASETS & API COMPAT LOAD FOR SUBPAGES
// ----------------------------------------------------
async function loadStaffList() {
    const tbody = document.getElementById("staff-table-body");
    const token = localStorage.getItem("token");
    
    try {
        const res = await fetch(`${API_URL}/auth/staff-list`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        
        let staff = [];
        if (res.ok) {
            const result = await res.json();
            staff = result.data || [];
        } else {
            // Mock matching the DB Seeder
            staff = [
                { fullName: "Nguyễn Manager", role: "Owner", phone: "0901234567", email: "owner@cafepos.com", baseSalary: 15000000, status: "Active" },
                { fullName: "Phạm ShiftLeader", role: "ShiftLeader", phone: "0902345678", email: "leader@cafepos.com", baseSalary: 8000000, status: "Active" },
                { fullName: "Trần Thu Ngân", role: "Cashier", phone: "0903456789", email: "cashier@cafepos.com", baseSalary: 6000000, status: "Active" },
                { fullName: "Lê Pha Chế", role: "Barista", phone: "0904567890", email: "barista@cafepos.com", baseSalary: 6500000, status: "Active" },
                { fullName: "Nguyễn Bánh Ngọt", role: "PastryStaff", phone: "0905678901", email: "pastry@cafepos.com", baseSalary: 6500000, status: "Active" }
            ];
        }
        
        tbody.innerHTML = staff.map(s => {
            const sal = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(s.baseSalary);
            const statusClass = s.status === "Active" ? "bg-success-light-badge" : "bg-danger-light-badge";
            
            return `
                <tr>
                    <td class="fw-bold">${s.fullName}</td>
                    <td><span class="badge bg-light text-dark border">${translateRole(s.role)}</span></td>
                    <td>${s.phone}</td>
                    <td>${s.email}</td>
                    <td class="fw-bold">${sal}</td>
                    <td><span class="badge ${statusClass}">Đang làm việc</span></td>
                </tr>
            `;
        }).join('');
    } catch(err) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center py-3 text-danger">Lỗi tải dữ liệu.</td></tr>`;
    }
}

async function loadVouchersList() {
    const tbody = document.getElementById("vouchers-table-body");
    
    // Vouchers mock data matching seed
    const vouchers = [
        { code: "SUMMER10", discountType: "Percent", discountValue: 10, minOrderValue: 50000, expiresAt: "2026-08-20T12:00:00Z", isActive: true },
        { code: "CAFE5K", discountType: "Fixed", discountValue: 5000, minOrderValue: 30000, expiresAt: "Vĩnh viễn", isActive: true }
    ];
    
    tbody.innerHTML = vouchers.map(v => {
        const valStr = v.discountType === "Percent" ? `${v.discountValue}%` : new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v.discountValue);
        const minStr = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v.minOrderValue);
        const exp = v.expiresAt === "Vĩnh viễn" ? "Vĩnh viễn" : new Date(v.expiresAt).toLocaleDateString("vi-VN");
        
        return `
            <tr>
                <td class="fw-bold text-success">${v.code}</td>
                <td>${v.discountType === "Percent" ? "Theo phần trăm" : "Giảm trực tiếp"}</td>
                <td class="fw-bold">${valStr}</td>
                <td>${minStr}</td>
                <td>${exp}</td>
                <td><span class="badge bg-success-light-badge">Hoạt động</span></td>
            </tr>
        `;
    }).join('');
}

async function loadInventoryList() {
    const tbody = document.getElementById("inventory-table-body");
    const token = localStorage.getItem("token");
    
    try {
        const res = await fetch(`${API_URL}/inventory`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        
        let ingredients = [];
        if (res.ok) {
            const result = await res.json();
            ingredients = result.data || [];
        } else {
            // Mock matching seed as fallback
            ingredients = [
                { name: "Trà đen Oolong", currentQuantity: 5.0, minAlertQuantity: 1.0, unit: "kg", expiresAt: "2026-12-20T12:00:00", isActive: true },
                { name: "Bột trà sữa", currentQuantity: 1.5, minAlertQuantity: 2.0, unit: "kg", expiresAt: "2026-12-20T12:00:00", isActive: true },
                { name: "Sữa đặc", currentQuantity: 4.0, minAlertQuantity: 6.0, unit: "lon", expiresAt: "2026-09-20T12:00:00", isActive: true },
                { name: "Hạt cà phê Robusta", currentQuantity: 15.0, minAlertQuantity: 3.0, unit: "kg", expiresAt: "2027-06-20T12:00:00", isActive: true },
                { name: "Bột Matcha Uji", currentQuantity: 0.3, minAlertQuantity: 0.5, unit: "kg", expiresAt: "2026-12-20T12:00:00", isActive: true }
            ];
        }
        
        tbody.innerHTML = ingredients.map(i => {
            const isLow = i.currentQuantity <= i.minAlertQuantity;
            const statusBadge = isLow ? "bg-danger-light-badge" : "bg-success-light-badge";
            const statusText = isLow ? "Hết/Sắp hết" : "Đủ hàng";
            const exp = new Date(i.expiresAt).toLocaleDateString("vi-VN");
            
            return `
                <tr>
                    <td class="fw-bold">${i.name}</td>
                    <td class="${isLow ? 'text-danger fw-bold' : ''}">${i.currentQuantity}</td>
                    <td>${i.minAlertQuantity}</td>
                    <td>${i.unit}</td>
                    <td>${exp}</td>
                    <td><span class="badge ${statusBadge}">${statusText}</span></td>
                </tr>
            `;
        }).join('');
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center py-3 text-danger">Lỗi kết nối máy chủ khi tải kho.</td></tr>`;
    }
}

// ----------------------------------------------------
// 5. SECURITY ACTIONS
// ----------------------------------------------------
function logoutAdmin() {
    localStorage.removeItem("token");
    localStorage.removeItem("staff");
    localStorage.removeItem("loginMode");
    window.location.href = "/Admin/Login";
}

// ========================================================
// 6. PRODUCTS & TOPPINGS CRUD OPERATIONS
// ========================================================

async function populateCategorySelect(selectId, selectedId = null) {
    const select = document.getElementById(selectId);
    const token = localStorage.getItem("token");
    try {
        const res = await fetch(`${API_URL}/products/categories`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await res.json();
        if (res.ok && result.success) {
            let html = '';
            result.data.forEach(c => {
                const isSel = c.id == selectedId ? 'selected' : '';
                html += `<option value="${c.id}" ${isSel}>${c.name}</option>`;
            });
            select.innerHTML = html;
        }
    } catch(e) {}
}

function openAddProductModal() {
    document.getElementById("productModalLabel").innerText = "Thêm sản phẩm mới";
    document.getElementById("product-id-input").value = "";
    document.getElementById("product-name-input").value = "";
    document.getElementById("product-price-input").value = "";
    document.getElementById("product-desc-input").value = "";
    document.getElementById("product-img-url").value = "";
    document.getElementById("product-img-preview").src = "https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=100&auto=format&fit=crop&q=60";
    document.getElementById("product-img-file").value = "";
    document.getElementById("product-status-input").value = "Active";
    document.getElementById("product-sugar-input").checked = true;
    document.getElementById("product-ice-input").checked = true;
    
    const sizeCheck = document.getElementById("product-size-input");
    sizeCheck.checked = false;
    toggleSizeFields(false);
    
    populateCategorySelect("product-category-input");

    const modal = new bootstrap.Modal(document.getElementById('productModal'));
    modal.show();
}

async function openEditProductModal(id) {
    const p = productsData.find(prod => prod.id === id);
    if (!p) return;
    
    document.getElementById("productModalLabel").innerText = "Chỉnh sửa sản phẩm";
    document.getElementById("product-id-input").value = p.id;
    document.getElementById("product-name-input").value = p.name;
    document.getElementById("product-price-input").value = p.basePrice;
    document.getElementById("product-desc-input").value = p.description;
    document.getElementById("product-img-url").value = p.imageUrl;
    document.getElementById("product-img-preview").src = p.imageUrl || "https://images.unsplash.com/photo-1509042239860-f550ce710b93?w=100&auto=format&fit=crop&q=60";
    document.getElementById("product-img-file").value = "";
    document.getElementById("product-status-input").value = p.status;
    document.getElementById("product-sugar-input").checked = p.hasSugarOption;
    document.getElementById("product-ice-input").checked = p.hasIceOption;
    
    const sizeCheck = document.getElementById("product-size-input");
    sizeCheck.checked = p.hasSizeOption;
    toggleSizeFields(p.hasSizeOption);
    
    const container = document.getElementById("size-rows-container");
    container.innerHTML = "";
    
    if (p.hasSizeOption && p.sizes && p.sizes.length > 0) {
        p.sizes.forEach(s => {
            addProductSizeRow(s.sizeLabel, s.priceModifier, s.isDefault);
        });
    }
    
    await populateCategorySelect("product-category-input", p.categoryId);

    const modal = new bootstrap.Modal(document.getElementById('productModal'));
    modal.show();
}

function toggleSizeFields(checked) {
    const panel = document.getElementById("size-options-panel");
    panel.style.display = checked ? "block" : "none";
    if (checked && document.getElementById("size-rows-container").children.length === 0) {
        addProductSizeRow("S", 0, true);
        addProductSizeRow("M", 5000, false);
        addProductSizeRow("L", 10000, false);
    }
}

function addProductSizeRow(label = "", modifier = 0, isDefault = false) {
    const container = document.getElementById("size-rows-container");
    const row = document.createElement("div");
    row.className = "row g-2 align-items-center mb-2 size-option-row";
    row.innerHTML = `
        <div class="col-4">
            <input type="text" class="form-control form-control-sm size-label-input" placeholder="VD: M" value="${label}" required />
        </div>
        <div class="col-4">
            <input type="number" class="form-control form-control-sm size-modifier-input" placeholder="VD: +5000" value="${modifier}" required />
        </div>
        <div class="col-3 text-center">
            <div class="form-check d-inline-block">
                <input class="form-check-input size-default-check" type="radio" name="size-default-radio" ${isDefault ? 'checked' : ''} />
                <label class="form-check-label small">Mặc định</label>
            </div>
        </div>
        <div class="col-1 text-end">
            <button type="button" class="btn btn-sm btn-link text-danger p-0" onclick="removeProductSizeRow(this)">Xóa</button>
        </div>
    `;
    container.appendChild(row);
}

function removeProductSizeRow(btn) {
    btn.closest(".size-option-row").remove();
}

async function uploadProductImage(input) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        const formData = new FormData();
        formData.append("file", file);
        
        const token = localStorage.getItem("token");
        
        try {
            const res = await fetch(`${API_URL}/products/upload-image`, {
                method: "POST",
                headers: { "Authorization": `Bearer ${token}` },
                body: formData
            });
            const result = await res.json();
            if (res.ok && result.success) {
                document.getElementById("product-img-url").value = result.data;
                document.getElementById("product-img-preview").src = result.data;
                showToast("Tải ảnh sản phẩm lên thành công!");
            } else {
                alert(result.message || "Tải ảnh lên thất bại.");
            }
        } catch(e) {
            alert("Lỗi kết nối tải ảnh.");
        }
    }
}

async function submitProductForm() {
    const id = document.getElementById("product-id-input").value;
    const name = document.getElementById("product-name-input").value.trim();
    const categoryId = parseInt(document.getElementById("product-category-input").value);
    const basePrice = parseFloat(document.getElementById("product-price-input").value);
    const description = document.getElementById("product-desc-input").value.trim();
    const imageUrl = document.getElementById("product-img-url").value;
    const status = document.getElementById("product-status-input").value;
    const hasSugarOption = document.getElementById("product-sugar-input").checked;
    const hasIceOption = document.getElementById("product-ice-input").checked;
    const hasSizeOption = document.getElementById("product-size-input").checked;
    const token = localStorage.getItem("token");

    if (!name || isNaN(basePrice) || isNaN(categoryId)) {
        alert("Vui lòng nhập đầy đủ các trường thông tin bắt buộc!");
        return;
    }

    const sizes = [];
    if (hasSizeOption) {
        const rows = document.querySelectorAll(".size-option-row");
        if (rows.length === 0) {
            alert("Bạn chưa định nghĩa size nào!");
            return;
        }
        
        let hasDefault = false;
        rows.forEach(r => {
            const sizeLabel = r.querySelector(".size-label-input").value.trim();
            const priceModifier = parseFloat(r.querySelector(".size-modifier-input").value);
            const isDefault = r.querySelector(".size-default-check").checked;
            
            if (sizeLabel && !isNaN(priceModifier)) {
                sizes.push({ sizeLabel, priceModifier, isDefault });
                if (isDefault) hasDefault = true;
            }
        });

        if (sizes.length > 0 && !hasDefault) {
            sizes[0].isDefault = true;
        }
    }

    const payload = {
        name, categoryId, basePrice, description, imageUrl, status,
        hasSugarOption, hasIceOption, hasSizeOption, sizes
    };

    const method = id ? "PUT" : "POST";
    const endpoint = id ? `${API_URL}/products/${id}` : `${API_URL}/products`;

    try {
        const res = await fetch(endpoint, {
            method: method,
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });
        
        const result = await res.json();
        if (res.ok && result.success) {
            showToast(result.message || "Lưu sản phẩm thành công!");
            
            const modalEl = document.getElementById('productModal');
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            if (modalInstance) modalInstance.hide();
            
            loadProducts();
        } else {
            alert(result.message || "Lỗi lưu sản phẩm.");
        }
    } catch(e) {
        alert("Lỗi kết nối.");
    }
}

async function deleteProduct(id) {
    if (!confirm("Bạn có chắc chắn muốn xóa sản phẩm này không?")) return;
    const token = localStorage.getItem("token");
    try {
        const res = await fetch(`${API_URL}/products/${id}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await res.json();
        if (res.ok && result.success) {
            showToast("Đã xóa sản phẩm thành công!");
            loadProducts();
        } else {
            alert(result.message || "Xóa sản phẩm thất bại.");
        }
    } catch(e) {
        alert("Lỗi kết nối xóa sản phẩm.");
    }
}

async function loadToppings() {
    const tbody = document.getElementById("toppings-table-body");
    const token = localStorage.getItem("token");
    
    try {
        const res = await fetch(`${API_URL}/products/toppings`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await res.json();
        if (res.ok && result.success) {
            toppingsData = result.data;
            renderToppingsTable();
        } else {
            tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-danger">Không thể tải toppings.</td></tr>`;
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-danger">Lỗi kết nối máy chủ API.</td></tr>`;
    }
}

function renderToppingsTable() {
    const tbody = document.getElementById("toppings-table-body");
    
    let filtered = toppingsData.filter(t => {
        return t.name.toLowerCase().includes(toppingSearchQuery.toLowerCase());
    });
    
    if (filtered.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center py-4 text-muted">Không tìm thấy topping nào.</td></tr>`;
        return;
    }
    
    tbody.innerHTML = filtered.map(t => {
        const price = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(t.price);
        const statusBadge = t.isActive ? "bg-success-light-badge" : "bg-secondary-light-badge";
        const statusText = t.isActive ? "Hoạt động" : "Ngừng bán";
        
        return `
            <tr>
                <td>
                    <input type="checkbox" class="form-check-input topping-row-check" data-id="${t.id}">
                </td>
                <td class="fw-bold text-dark">${t.name}</td>
                <td class="fw-bold text-success">${price}</td>
                <td><span class="badge ${statusBadge}">${statusText}</span></td>
                <td class="text-end">
                    <button class="btn btn-sm btn-link text-success p-0 me-3" onclick="openEditToppingModal(${t.id})">Sửa</button>
                    <button class="btn btn-sm btn-link text-danger p-0 me-2" onclick="deleteTopping(${t.id})">Xóa</button>
                </td>
            </tr>
        `;
    }).join('');
}

function handleToppingSearch(val) {
    toppingSearchQuery = val;
    renderToppingsTable();
}

function toggleSelectAllToppings(masterCheck) {
    const checkboxes = document.querySelectorAll(".topping-row-check");
    checkboxes.forEach(cb => cb.checked = masterCheck.checked);
}

function openAddToppingModal() {
    document.getElementById("toppingModalLabel").innerText = "Thêm topping mới";
    document.getElementById("topping-id-input").value = "";
    document.getElementById("topping-name-input").value = "";
    document.getElementById("topping-price-input").value = "";
    document.getElementById("topping-active-input").checked = true;
    
    const modal = new bootstrap.Modal(document.getElementById('toppingModal'));
    modal.show();
}

function openEditToppingModal(id) {
    const t = toppingsData.find(top => top.id === id);
    if (!t) return;
    
    document.getElementById("toppingModalLabel").innerText = "Chỉnh sửa topping";
    document.getElementById("topping-id-input").value = t.id;
    document.getElementById("topping-name-input").value = t.name;
    document.getElementById("topping-price-input").value = t.price;
    document.getElementById("topping-active-input").checked = t.isActive;
    
    const modal = new bootstrap.Modal(document.getElementById('toppingModal'));
    modal.show();
}

async function submitToppingForm() {
    const id = document.getElementById("topping-id-input").value;
    const name = document.getElementById("topping-name-input").value.trim();
    const price = parseFloat(document.getElementById("topping-price-input").value);
    const isActive = document.getElementById("topping-active-input").checked;
    const token = localStorage.getItem("token");

    if (!name || isNaN(price)) {
        alert("Vui lòng điền đầy đủ các thông tin!");
        return;
    }

    const payload = { name, price, isActive };
    const method = id ? "PUT" : "POST";
    const endpoint = id ? `${API_URL}/products/toppings/${id}` : `${API_URL}/products/toppings`;

    try {
        const res = await fetch(endpoint, {
            method: method,
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });
        const result = await res.json();
        if (res.ok && result.success) {
            showToast(result.message || "Lưu topping thành công!");
            
            const modalEl = document.getElementById('toppingModal');
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            if (modalInstance) modalInstance.hide();
            
            loadToppings();
        } else {
            alert(result.message || "Lỗi lưu topping.");
        }
    } catch(e) {
        alert("Lỗi kết nối.");
    }
}

async function deleteTopping(id) {
    if (!confirm("Bạn có chắc chắn muốn xóa topping này không?")) return;
    const token = localStorage.getItem("token");
    try {
        const res = await fetch(`${API_URL}/products/toppings/${id}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await res.json();
        if (res.ok && result.success) {
            showToast("Đã xóa topping thành công!");
            loadToppings();
        } else {
            alert(result.message || "Xóa topping thất bại.");
        }
    } catch(e) {
        alert("Lỗi kết nối.");
    }
}
