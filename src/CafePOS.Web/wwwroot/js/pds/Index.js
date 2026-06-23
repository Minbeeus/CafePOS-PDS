import { apiClient } from '../shared/api-client.js';
import { signalRService } from '../shared/signalr.service.js';
import { toastService } from '../shared/toast.service.js';

let currentStation = 'Bar';
let ordersList = [];

// Initialize Page
document.addEventListener('DOMContentLoaded', async () => {
    // Apply body dark theme dynamically
    document.body.classList.add('pds-body-bg');

    // 1. Resolve Current Station from query string & check role limits
    const urlParams = new URLSearchParams(window.location.search);
    currentStation = urlParams.get('station') || 'Bar';

    const staffStr = localStorage.getItem("staff");
    let userRole = "";
    if (staffStr) {
        try {
            userRole = JSON.parse(staffStr).role;
        } catch (_) {}
    }

    if (userRole === 'Barista') {
        currentStation = 'Bar';
    } else if (userRole === 'PastryStaff') {
        currentStation = 'Pastry';
    }

    // 2. Setup Header UI
    setupHeaderUI();

    // 3. Start Live Clock
    startClock();

    // 4. Fetch initial orders
    await loadInitialOrders();

    // 5. Setup SignalR Connection
    await setupSignalR();

    // 6. Start ticking active order elapsed times
    setInterval(updateTimers, 1000);
});

function setupHeaderUI() {
    const badge = document.getElementById('pdsStationBadge');
    const btnBar = document.getElementById('btnStationBar');
    const btnPastry = document.getElementById('btnStationPastry');

    const staffStr = localStorage.getItem("staff");
    let userRole = "";
    if (staffStr) {
        try {
            userRole = JSON.parse(staffStr).role;
        } catch (_) {}
    }

    // Hide station switching buttons for Barista and PastryStaff to prevent unauthorized switching
    if (userRole === 'Barista' || userRole === 'PastryStaff') {
        if (btnBar) btnBar.style.display = 'none';
        if (btnPastry) btnPastry.style.display = 'none';
    }

    if (currentStation.toLowerCase() === 'pastry') {
        currentStation = 'Pastry';
        badge.innerText = 'Quầy Bánh Ngọt';
        badge.className = 'badge bg-dark border border-secondary text-warning fs-6 py-2 px-3';
        if (btnPastry) {
            btnPastry.classList.add('active', 'text-warning');
            btnPastry.classList.remove('text-white-50');
        }
    } else {
        currentStation = 'Bar';
        badge.innerText = 'Quầy Pha Chế';
        badge.className = 'badge bg-dark border border-secondary text-info fs-6 py-2 px-3';
        if (btnBar) {
            btnBar.classList.add('active', 'text-info');
            btnBar.classList.remove('text-white-50');
        }
    }
}

function startClock() {
    const clockEl = document.getElementById('liveClock');
    setInterval(() => {
        const now = new Date();
        clockEl.innerText = now.toTimeString().split(' ')[0];
    }, 1000);
}

// Fetch confirmed, preparing, and completed orders from API
async function loadInitialOrders() {
    try {
        // Fetch confirmed/preparing orders
        const activeData = await apiClient.get('/orders?status=Confirmed,Preparing&pageSize=100');
        
        // Fetch recently completed orders of today
        const completedData = await apiClient.get('/orders?status=Completed&date=today&pageSize=15');

        // Filter and map active and completed orders
        ordersList = [...(activeData.items || []), ...(completedData.items || [])];

        renderBoard();
    } catch (err) {
        console.error('Failed to load orders:', err);
        toastService.danger('Không thể tải danh sách đơn hàng ban đầu.');
    }
}

// Filter items inside an order that belong to the current station
function getStationItems(order) {
    if (!order.items) return [];
    return order.items.filter(item => {
        if (currentStation === 'Bar') {
            return item.barStatus !== 'NA';
        } else {
            return item.pastryStatus !== 'NA';
        }
    });
}

// Render the 3-column preparation board based on station-specific item status
function renderBoard() {
    const colQueued = document.getElementById('colQueued');
    const colPreparing = document.getElementById('colPreparing');
    const colCompleted = document.getElementById('colCompleted');

    // Clear board columns
    colQueued.innerHTML = '';
    colPreparing.innerHTML = '';
    colCompleted.innerHTML = '';

    let queuedCount = 0;
    let preparingCount = 0;
    let completedCount = 0;

    ordersList.forEach(order => {
        const stationItems = getStationItems(order);
        if (stationItems.length === 0) return; // Order doesn't contain items for this station

        // Determine column placement based on the status of the current station's items
        let isAllDone = order.status === 'Completed';
        let isAnyPreparing = false;

        if (!isAllDone) {
            isAllDone = true;
            stationItems.forEach(item => {
                const status = currentStation === 'Bar' ? item.barStatus : item.pastryStatus;
                if (status !== 'Done') {
                    isAllDone = false;
                }
                if (status === 'Preparing') {
                    isAnyPreparing = true;
                }
            });
        }

        const card = createOrderCard(order, stationItems);

        if (isAllDone) {
            colCompleted.appendChild(card);
            completedCount++;
        } else if (isAnyPreparing) {
            colPreparing.appendChild(card);
            preparingCount++;
        } else {
            colQueued.appendChild(card);
            queuedCount++;
        }
    });

    document.getElementById('countQueued').innerText = queuedCount;
    document.getElementById('countPreparing').innerText = preparingCount;
    document.getElementById('countCompleted').innerText = completedCount;
}

// Helper to create card DOM element
function createOrderCard(order, items) {
    const card = document.createElement('div');
    card.className = 'pds-card';
    card.id = `order-card-${order.id}`;
    card.dataset.confirmedAt = order.confirmedAt || order.createdAt;

    // Determine station-specific preparation status
    let isAllDone = order.status === 'Completed';
    let isAnyPreparing = false;

    if (!isAllDone) {
        isAllDone = true;
        items.forEach(item => {
            const status = currentStation === 'Bar' ? item.barStatus : item.pastryStatus;
            if (status !== 'Done') {
                isAllDone = false;
            }
            if (status === 'Preparing') {
                isAnyPreparing = true;
            }
        });
    }

    if (isAllDone) {
        card.dataset.completedAt = order.completedAt || new Date().toISOString();
    }

    // Card Header
    const header = document.createElement('div');
    header.className = 'pds-card-header';
    header.innerHTML = `
        <div>
            <span class="fw-bold text-white fs-5">${order.orderCode.substring(order.orderCode.length - 6)}</span>
            <span class="badge bg-secondary ms-2">${order.type === 'DineIn' ? `Bàn ${order.tableNumber || '?'}` : 'Mang đi'}</span>
        </div>
        <span class="pds-time-elapsed" id="timer-${order.id}">0m</span>
    `;

    // Card Body
    const body = document.createElement('div');
    body.className = 'pds-card-body';
    
    // Items
    items.forEach(item => {
        const itemEl = document.createElement('div');
        itemEl.className = 'mb-3';
        
        let optionsHtml = '';
        if (item.sizeLabel && item.sizeLabel !== 'Regular' && item.sizeLabel !== 'Normal') {
            optionsHtml += `<li>Size: ${item.sizeLabel}</li>`;
        }
        if (item.sugarLevel && item.sugarLevel !== '100' && item.sugarLevel !== 'NA') {
            optionsHtml += `<li>Đường: ${item.sugarLevel}%</li>`;
        }
        if (item.iceLevel && item.iceLevel !== '100' && item.iceLevel !== 'NA') {
            optionsHtml += `<li>Đá: ${item.iceLevel}%</li>`;
        }
        if (item.toppings && item.toppings.length > 0) {
            optionsHtml += `<li>Topping: ${item.toppings.map(t => t.toppingName).join(', ')}</li>`;
        }

        itemEl.innerHTML = `
            <div class="d-flex align-items-center gap-2">
                <span class="pds-item-badge">${item.quantity}</span>
                <span class="fw-bold text-light">${item.productName}</span>
            </div>
            ${optionsHtml ? `<ul class="pds-item-options mb-1">${optionsHtml}</ul>` : ''}
            ${item.notes ? `<div class="pds-item-notes">${item.notes}</div>` : ''}
        `;
        body.appendChild(itemEl);
    });

    card.appendChild(header);
    card.appendChild(body);

    // Card Footer Actions
    if (!isAllDone) {
        const footer = document.createElement('div');
        footer.className = 'pds-card-footer';
        
        const btn = document.createElement('button');
        btn.className = `btn btn-sm fw-bold rounded-pill px-4 ${!isAnyPreparing ? 'btn-info' : 'btn-success'}`;
        btn.innerText = !isAnyPreparing ? '▶ Bắt đầu làm' : '✔ Hoàn thành';
        btn.onclick = () => handleStatusUpdate(order.id, !isAnyPreparing ? 'Preparing' : 'Completed');
        
        footer.appendChild(btn);
        card.appendChild(footer);
    }

    return card;
}

// PUT request to update order station prep status
async function handleStatusUpdate(orderId, newStatus) {
    try {
        await apiClient.put(`/orders/${orderId}/station-status`, { 
            station: currentStation, 
            status: newStatus 
        });
        toastService.success(`Đơn hàng #${orderId} tại quầy ${currentStation === 'Bar' ? 'Pha chế' : 'Bánh'} chuyển sang ${newStatus === 'Preparing' ? 'Đang thực hiện' : 'Hoàn thành'}.`);
        
        // Update local order item status
        const order = ordersList.find(o => o.id === orderId);
        if (order) {
            const itemStatusValue = newStatus === 'Preparing' ? 'Preparing' : 'Done';
            order.items.forEach(item => {
                if (currentStation === 'Bar' && item.barStatus !== 'NA') {
                    item.barStatus = itemStatusValue;
                } else if (currentStation === 'Pastry' && item.pastryStatus !== 'NA') {
                    item.pastryStatus = itemStatusValue;
                }
            });

            // Re-evaluate overall order status locally based on updated items
            let allDone = true;
            let anyPreparingOrDone = false;
            order.items.forEach(item => {
                const itemBarDone = item.barStatus === 'Done' || item.barStatus === 'NA';
                const itemPastryDone = item.pastryStatus === 'Done' || item.pastryStatus === 'NA';
                if (!itemBarDone || !itemPastryDone) {
                    allDone = false;
                }
                if (item.barStatus === 'Preparing' || item.barStatus === 'Done' ||
                    item.pastryStatus === 'Preparing' || item.pastryStatus === 'Done') {
                    anyPreparingOrDone = true;
                }
            });

            if (allDone) {
                order.status = 'Completed';
                order.completedAt = new Date().toISOString();
            } else if (anyPreparingOrDone) {
                order.status = 'Preparing';
                if (newStatus === 'Preparing') {
                    order.confirmedAt = new Date().toISOString();
                }
            } else {
                order.status = 'Confirmed';
            }

            renderBoard();
        }
    } catch (err) {
        console.error('Failed to update status:', err);
        toastService.danger(err.message || 'Không thể cập nhật trạng thái đơn hàng.');
    }
}

// Utility to parse UTC date strings securely in the browser
function parseUtcDate(dateStr) {
    if (!dateStr) return null;
    if (!dateStr.endsWith('Z') && !/[+-]\d{2}:\d{2}$/.test(dateStr)) {
        return new Date(dateStr + 'Z');
    }
    return new Date(dateStr);
}

// Tick elapsed timers
function updateTimers() {
    const cards = document.querySelectorAll('.pds-card');
    cards.forEach(card => {
        const id = card.id.replace('order-card-', '');
        const confirmedAtStr = card.dataset.confirmedAt;
        if (!confirmedAtStr) return;

        const confirmedAt = parseUtcDate(confirmedAtStr);
        let endTime = new Date();
        
        if (card.dataset.completedAt) {
            endTime = parseUtcDate(card.dataset.completedAt);
        }

        const diffMs = endTime - confirmedAt;
        const diffMin = Math.floor(diffMs / 60000);
        const diffSec = Math.floor((diffMs % 60000) / 1000);

        const timerEl = document.getElementById(`timer-${id}`);
        if (timerEl) {
            timerEl.innerText = `${diffMin}m ${diffSec}s`;
            if (card.dataset.completedAt) {
                timerEl.style.color = '#28a745'; // green for completed
            } else if (diffMin >= 10) {
                timerEl.style.color = '#dc3545'; // Highlight red if elapsed time exceeds 10m
            } else if (diffMin >= 5) {
                timerEl.style.color = '#ffc107'; // Highlight yellow if elapsed time exceeds 5m
            } else {
                timerEl.style.color = '#fd7e14';
            }
        }
    });
}

// Setup SignalR Hub Listeners
async function setupSignalR() {
    const dot = document.getElementById('signalrStatusDot');
    const txt = document.getElementById('signalrStatusText');

    // Register Listener: Order Paid & Confirmed
    signalRService.registerListener('OrderConfirmed', (order) => {
        console.log('SignalR: OrderConfirmed received', order);
        toastService.info(`Đơn hàng mới #${order.orderCode.substring(order.orderCode.length - 6)} vừa được tạo!`);
        
        // Add to local list and rerender
        if (!ordersList.some(o => o.id === order.id)) {
            ordersList.push(order);
            renderBoard();
            
            // Add a visual flash effect on the newly appended card
            const card = document.getElementById(`order-card-${order.id}`);
            if (card) {
                card.classList.add('pds-card-new');
                setTimeout(() => card.classList.remove('pds-card-new'), 5000);
            }
        }
    });

    // Register Listener: Order Status Modified globally
    signalRService.registerListener('OrderStatusChanged', (orderId, status) => {
        console.log('SignalR: OrderStatusChanged received', orderId, status);
        const order = ordersList.find(o => o.id === orderId);
        if (order) {
            order.status = status;
            if (status === 'Completed') {
                order.completedAt = new Date().toISOString();
                order.items.forEach(item => {
                    if (item.barStatus !== 'NA') item.barStatus = 'Done';
                    if (item.pastryStatus !== 'NA') item.pastryStatus = 'Done';
                });
            } else if (status === 'Cancelled') {
                ordersList = ordersList.filter(o => o.id !== orderId);
            }
            renderBoard();
        }
    });

    // Start connection
    const conn = await signalRService.startConnection();
    if (conn) {
        dot.className = 'pds-status-dot connected';
        txt.innerText = 'Đã kết nối';
        
        // Join specific group station (extensible)
        await signalRService.joinGroup(currentStation);
    } else {
        dot.className = 'pds-status-dot disconnected';
        txt.innerText = 'Lỗi kết nối';
    }
}
