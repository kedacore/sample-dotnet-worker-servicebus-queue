"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (orderAmount) {
    var orderAmountElement = document.getElementById("order-amount");

    var totalAmount = 0;
    if (orderAmountElement.innerText) {
        totalAmount = parseInt(orderAmountElement.innerText, 10);
    }
    totalAmount += parseInt(orderAmount,10);
    orderAmountElement.innerText = totalAmount;

    var ordersConsumedElement = document.getElementById("orders-consumed");
    var ordersConsumed = 0;
    if (ordersConsumedElement.innerText) {
        ordersConsumed = parseInt(ordersConsumedElement.innerText, 10);
    }
    ordersConsumed += 1;
    ordersConsumedElement.innerText = ordersConsumed;

});

connection.start().then(function () {

}).catch(function (err) {
    return console.error(err.toString());
});
