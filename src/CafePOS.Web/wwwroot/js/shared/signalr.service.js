export const signalRService = {
    connection: null,
    
    startConnection: async function (hubUrl = 'http://localhost:5076/hubs/orders') {
        if (this.connection) return this.connection;
        
        if (typeof signalR === 'undefined') {
            console.error('SignalR client library is not loaded. Please include signalr.js first.');
            return null;
        }
        
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                withCredentials: true
            })
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();
            
        this.connection.onclose(error => {
            console.warn('SignalR connection closed:', error);
        });
        
        this.connection.onreconnecting(error => {
            console.warn('SignalR reconnecting due to error:', error);
        });
        
        this.connection.onreconnected(connectionId => {
            console.log('SignalR reconnected. Connection ID:', connectionId);
        });
        
        try {
            await this.connection.start();
            console.log('SignalR connected. Connection ID:', this.connection.connectionId);
            return this.connection;
        } catch (err) {
            console.error('Error establishing SignalR connection:', err);
            // Retry after 5s
            setTimeout(() => this.startConnection(hubUrl), 5000);
            return null;
        }
    },
    
    registerListener: function (eventName, callback) {
        if (this.connection) {
            this.connection.on(eventName, callback);
        } else {
            console.warn('SignalR connection not initialized yet. Listener registry failed.');
        }
    },
    
    joinGroup: async function (groupName) {
        if (this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
            try {
                await this.connection.invoke('JoinStationGroup', groupName);
                console.log(`Joined SignalR station group: ${groupName}`);
            } catch (err) {
                console.error(`Failed to join group ${groupName}:`, err);
            }
        }
    }
};
