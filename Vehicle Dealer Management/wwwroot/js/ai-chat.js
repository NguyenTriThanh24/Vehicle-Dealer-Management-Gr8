// AI Chat Component JavaScript
(function() {
    'use strict';

    let chatContainer = null;
    let chatWindow = null;
    let chatBubble = null;
    let messagesContainer = null;
    let inputField = null;
    let sendButton = null;
    let isOpen = false;

    function initChat() {
        // Tạo chat container
        chatContainer = document.createElement('div');
        chatContainer.className = 'ai-chat-container';

        // Tạo chat bubble
        chatBubble = document.createElement('div');
        chatBubble.className = 'ai-chat-bubble';
        chatBubble.innerHTML = '<i class="bi bi-robot"></i>';
        chatBubble.setAttribute('title', 'AI Assistant');
        chatBubble.addEventListener('click', toggleChat);

        // Tạo chat window
        chatWindow = document.createElement('div');
        chatWindow.className = 'ai-chat-window';

        // Header
        const header = document.createElement('div');
        header.className = 'ai-chat-header';
        header.innerHTML = `
            <div class="ai-chat-header-title">
                <i class="bi bi-robot"></i>
                <span>AI Assistant</span>
            </div>
            <button class="ai-chat-close" type="button">
                <i class="bi bi-x-lg"></i>
            </button>
        `;
        header.querySelector('.ai-chat-close').addEventListener('click', toggleChat);

        // Messages container
        messagesContainer = document.createElement('div');
        messagesContainer.className = 'ai-chat-messages';

        // Input container
        const inputContainer = document.createElement('div');
        inputContainer.className = 'ai-chat-input-container';

        inputField = document.createElement('textarea');
        inputField.className = 'ai-chat-input';
        inputField.placeholder = 'Nhập câu hỏi của bạn...';
        inputField.rows = 1;
        inputField.addEventListener('keydown', handleKeyDown);
        inputField.addEventListener('input', autoResize);

        sendButton = document.createElement('button');
        sendButton.className = 'ai-chat-send';
        sendButton.innerHTML = '<i class="bi bi-send-fill"></i>';
        sendButton.addEventListener('click', sendMessage);

        inputContainer.appendChild(inputField);
        inputContainer.appendChild(sendButton);

        // Assemble
        chatWindow.appendChild(header);
        chatWindow.appendChild(messagesContainer);
        chatWindow.appendChild(inputContainer);

        chatContainer.appendChild(chatBubble);
        chatContainer.appendChild(chatWindow);

        // Thêm welcome message
        addMessage('ai', 'Xin chào! Tôi là AI Assistant. Bạn cần hỗ trợ gì?');

        // Append to body
        document.body.appendChild(chatContainer);
    }

    function toggleChat() {
        isOpen = !isOpen;
        if (isOpen) {
            chatWindow.classList.add('open');
            inputField.focus();
        } else {
            chatWindow.classList.remove('open');
        }
    }

    function handleKeyDown(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    }

    function autoResize() {
        inputField.style.height = 'auto';
        inputField.style.height = Math.min(inputField.scrollHeight, 100) + 'px';
    }

    async function sendMessage() {
        const message = inputField.value.trim();
        if (!message || sendButton.disabled) {
            return;
        }

        // Add user message
        addMessage('user', message);

        // Clear input
        inputField.value = '';
        inputField.style.height = 'auto';
        sendButton.disabled = true;
        inputField.disabled = true;

        // Show loading
        const loadingId = addLoadingMessage();

        try {
            // Get CSRF token if available
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';

            // Send request
            const response = await fetch('/Admin/AIChat?handler=Chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ message: message })
            });

            // Remove loading
            removeLoadingMessage(loadingId);

            if (!response.ok) {
                throw new Error('Failed to get response');
            }

            const data = await response.json();
            
            if (data.error) {
                addMessage('ai', 'Xin lỗi, đã có lỗi xảy ra: ' + data.error);
            } else {
                addMessage('ai', data.response || 'Không có phản hồi.');
            }
        } catch (error) {
            console.error('Error sending message:', error);
            removeLoadingMessage(loadingId);
            addMessage('ai', 'Xin lỗi, không thể kết nối đến server. Vui lòng thử lại sau.');
        } finally {
            sendButton.disabled = false;
            inputField.disabled = false;
            inputField.focus();
        }
    }

    function addMessage(type, content) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `ai-chat-message ${type}`;

        const avatar = document.createElement('div');
        avatar.className = 'ai-chat-message-avatar';
        avatar.textContent = type === 'user' ? 'U' : 'AI';

        const contentDiv = document.createElement('div');
        contentDiv.className = 'ai-chat-message-content';
        contentDiv.textContent = content;

        messageDiv.appendChild(avatar);
        messageDiv.appendChild(contentDiv);

        messagesContainer.appendChild(messageDiv);
        scrollToBottom();
    }

    function addLoadingMessage() {
        const loadingDiv = document.createElement('div');
        loadingDiv.className = 'ai-chat-loading';
        loadingDiv.id = 'ai-chat-loading-' + Date.now();
        loadingDiv.innerHTML = `
            <div class="ai-chat-loading-dot"></div>
            <div class="ai-chat-loading-dot"></div>
            <div class="ai-chat-loading-dot"></div>
            <span>Đang xử lý...</span>
        `;
        messagesContainer.appendChild(loadingDiv);
        scrollToBottom();
        return loadingDiv.id;
    }

    function removeLoadingMessage(id) {
        const loading = document.getElementById(id);
        if (loading) {
            loading.remove();
        }
    }

    function scrollToBottom() {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initChat);
    } else {
        initChat();
    }
})();

