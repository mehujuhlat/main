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

// Vastaanota viesti
connection.on("ReceiveMessage", (senderId, senderName, receiverId, receiverName, messageId, message, date, owner, private) => {
    const li = document.createElement("li");

    li.textContent = `${date} < ${senderName} > ${receiverName} : ${message}`;

    if (owner)
        li.innerHTML += `<a href="/MyMessages/Delete/${messageId}">[x]</a>`;

    if (private)
        document.getElementById("messagesPrivateList").appendChild(li);
    else 
        document.getElementById("messagesList").appendChild(li);
});

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
