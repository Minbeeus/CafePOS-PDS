// Dynamic CSS injector for Toast notifications
function injectStyles() {
    if (document.getElementById('toast-styles')) return;
    const style = document.createElement('style');
    style.id = 'toast-styles';
    style.innerHTML = `
        .toast-container-custom {
            position: fixed;
            top: 24px;
            right: 24px;
            z-index: 9999;
            display: flex;
            flex-direction: column;
            gap: 12px;
        }
        .toast-item-custom {
            min-width: 320px;
            max-width: 450px;
            background: rgba(30, 30, 30, 0.85);
            backdrop-filter: blur(12px);
            color: #fff;
            padding: 16px 20px;
            border-radius: 12px;
            border: 1px solid rgba(255, 255, 255, 0.1);
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
            display: flex;
            align-items: center;
            justify-content: space-between;
            gap: 12px;
            opacity: 0;
            transform: translateX(100%) scale(0.9);
            transition: all 0.35s cubic-bezier(0.175, 0.885, 0.32, 1.275);
        }
        .toast-item-custom.show {
            opacity: 1;
            transform: translateX(0) scale(1);
        }
        .toast-item-custom.info {
            border-left: 5px solid #0dcaf0;
        }
        .toast-item-custom.success {
            border-left: 5px solid #198754;
        }
        .toast-item-custom.warning {
            border-left: 5px solid #ffc107;
        }
        .toast-item-custom.danger {
            border-left: 5px solid #dc3545;
        }
        .toast-content-custom {
            font-size: 0.95rem;
            font-weight: 500;
            flex-grow: 1;
        }
        .toast-close-custom {
            background: transparent;
            border: none;
            color: rgba(255, 255, 255, 0.5);
            font-size: 1.2rem;
            cursor: pointer;
            line-height: 1;
            padding: 0;
            transition: color 0.15s ease;
        }
        .toast-close-custom:hover {
            color: #fff;
        }
    `;
    document.head.appendChild(style);
}

function getContainer() {
    let container = document.getElementById('toast-container-custom');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container-custom';
        container.className = 'toast-container-custom';
        document.body.appendChild(container);
    }
    return container;
}

export const toastService = {
    show: (message, type = 'info', duration = 4000) => {
        injectStyles();
        const container = getContainer();
        
        const toast = document.createElement('div');
        toast.className = `toast-item-custom ${type}`;
        
        const content = document.createElement('div');
        content.className = 'toast-content-custom';
        content.innerText = message;
        
        const closeBtn = document.createElement('button');
        closeBtn.className = 'toast-close-custom';
        closeBtn.innerHTML = '&times;';
        closeBtn.onclick = () => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 400);
        };
        
        toast.appendChild(content);
        toast.appendChild(closeBtn);
        container.appendChild(toast);
        
        // Trigger show animation
        requestAnimationFrame(() => {
            toast.classList.add('show');
        });
        
        if (duration > 0) {
            setTimeout(() => {
                toast.classList.remove('show');
                setTimeout(() => toast.remove(), 400);
            }, duration);
        }
    },
    success: (message, duration) => toastService.show(message, 'success', duration),
    info: (message, duration) => toastService.show(message, 'info', duration),
    warning: (message, duration) => toastService.show(message, 'warning', duration),
    danger: (message, duration) => toastService.show(message, 'danger', duration)
};
