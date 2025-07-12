const connection = new signalR.HubConnectionBuilder()
    .withUrl("/msgHub")
    .build();

// Yhdistä chattiin
connection.start()
    .then(() => console.log("Connected to chat hub"))
    .catch(err => console.error(err));

// Lähetä viesti
document.getElementById("sendButton").addEventListener("click", function () {
    const receiverId = document.getElementById("receiverId").value;
    const message = document.getElementById("messageInput").value;
    const private = document.getElementById("PrivateInput").checked;
    document.getElementById("messageInput").value = ``;
    connection.invoke("SendMessage", receiverId, message, private)
        .catch(err => console.error(err));
});

window.setReceiver = function setReceiver(id) {
    document.getElementById("receiverId").value = id;
    document.getElementById("messageInput").focus();
    document.getElementById("PrivateInput").checked = true;
}

// Vastaanota viesti
connection.on("ReceiveMessage", (senderId, senderName, receiverId, receiverName, messageId, message, date, owner, isPrivate) => {
    const li = document.createElement("li");

    const safeDate = escapeHtml(date);
    const safeSenderName = escapeHtml(senderName);
    const safeReceiverName = escapeHtml(receiverName);
    const safeMessage = escapeHtml(message);

    li.innerHTML = `${safeDate} &lt; `;

    // Lisää lähettäjä linkkinä tai tekstinä
    if (owner) {
        li.innerHTML += `${safeSenderName} &gt;`;
    } else {
        li.innerHTML += `<a class="message-user-link" onclick="setReceiver(${escapeHtml(senderId.toString())}">${safeSenderName}</a> &gt;`;
    }

    // Lisää vastaanottaja linkkinä
    li.innerHTML += `<a class="message-user-link" onclick="setReceiver(${escapeHtml(receiverId.toString())}">${safeReceiverName}</a> : ${safeMessage}`;

    // Lisää poistolinkki omistajalle
    if (owner) {
        li.innerHTML += ` <a href="/MyMessages/Delete/${escapeHtml(messageId.toString())}">[x]</a>`;
    }

    // Lisää viesti oikeaan listaan
    let container, list;
    if (isPrivate) {
        list = document.getElementById("messagesPrivateList");
        container = document.getElementById("privateMessagesContainer");
    } else {
        list = document.getElementById("messagesList");
        container = document.getElementById("globalMessagesContainer");
    }
    list.appendChild(li);

    // Käsittele automaattiskrollaus
    const scrollThreshold = 50;
    const isNearBottom = container.scrollHeight - container.clientHeight - container.scrollTop <= scrollThreshold;
    if (isNearBottom) {
        container.scrollTop = container.scrollHeight;
    }
});

function escapeHtml(unsafe) {
    if (!unsafe) return '';
    return unsafe.toString()
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

//AI koodia
document.addEventListener('DOMContentLoaded', function () {
    const toggleButtons = document.querySelectorAll('.toggle-chat');

    toggleButtons.forEach(button => {
        const targetId = button.getAttribute('data-target');
        const container = document.getElementById(targetId);

        if (container.classList.contains('hidden')) {
            button.textContent = '[Näytä]';
        } else {
            button.textContent = '[Piilota]';
        }

        button.addEventListener('click', function () {
            container.classList.toggle('hidden');

            if (container.classList.contains('hidden')) {
                button.textContent = '[Näytä]';
            } else {
                button.textContent = '[Piilota]';
                container.scrollTop = container.scrollHeight;
            }
        });
    });
    scrollContainersToBottom();
});

function scrollContainersToBottom() {
    const privateContainer = document.getElementById('privateMessagesContainer');
    const globalContainer = document.getElementById('globalMessagesContainer');

    if (privateContainer && !privateContainer.classList.contains('hidden')) {
        privateContainer.scrollTop = privateContainer.scrollHeight;
    }

    if (globalContainer && !globalContainer.classList.contains('hidden')) {
        globalContainer.scrollTop = globalContainer.scrollHeight;
    }
}







/*
document.addEventListener('DOMContentLoaded', function () {
    const privateContainer = document.getElementById('privateMessagesContainer');
    const globalContainer = document.getElementById('globalMessagesContainer');

    privateContainer.scrollTop = privateContainer.scrollHeight;
    globalContainer.scrollTop = globalContainer.scrollHeight;
});*/

/*
connection.on("ReceivePrivateMessage", (senderId, senderName, receiverId, receiverName, messageId, message, date, owner) => {
    const li = document.createElement("li");
    if (!owner)
        li.textContent = `${date} < ${senderName} > ${receiverName} : ${message}`;
    else
        li.innerHTML = `${date} < ${senderName} > ${receiverName} : ${message} <a href="/MyMessages/Delete/${messageId}">[x]</a>`;
    document.getElementById("messagesPrivateList").appendChild(li);
});
*/
