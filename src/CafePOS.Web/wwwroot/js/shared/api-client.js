const BASE_URL = 'http://localhost:5076/api/v1';

async function request(endpoint, options = {}) {
    const token = localStorage.getItem('token');
    
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers,
    };
    
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    
    const response = await fetch(`${BASE_URL}${endpoint}`, {
        ...options,
        headers,
    });
    
    if (!response.ok) {
        let errMsg = `Giao dịch thất bại: ${response.status}`;
        try {
            const errData = await response.json();
            errMsg = errData.message || errMsg;
        } catch (_) {}
        throw new Error(errMsg);
    }
    
    if (response.status === 204) {
        return null;
    }
    
    return await response.json();
}

export const apiClient = {
    get: (endpoint) => request(endpoint, { method: 'GET' }),
    post: (endpoint, data) => request(endpoint, { method: 'POST', body: JSON.stringify(data) }),
    put: (endpoint, data) => request(endpoint, { method: 'PUT', body: JSON.stringify(data) }),
    patch: (endpoint, data) => request(endpoint, { method: 'PATCH', body: JSON.stringify(data) }),
    delete: (endpoint) => request(endpoint, { method: 'DELETE' }),
};
