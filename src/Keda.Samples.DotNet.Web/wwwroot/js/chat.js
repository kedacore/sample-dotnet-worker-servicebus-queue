"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (orderAmount) {
    var li = document.getElementById("order-amount");

    var totalAmount = 0;
    if (!!li.innerText) {
        totalAmount = parseInt(li.innerText, 10);
    }
    totalAmount = totalAmount + parseInt(orderAmount,10);
    li.innerText = totalAmount;

    var li2 = document.getElementById("orders-consumed");
    var ordersConsumed = 0;
    if (!!li2.innerText) {
        ordersConsumed = parseInt(li2.innerText, 10);
    }
    ordersConsumed = ordersConsumed + 1;
    li2.innerText = ordersConsumed;

});

connection.start().then(function () {

}).catch(function (err) {
    return console.error(err.toString());
});
