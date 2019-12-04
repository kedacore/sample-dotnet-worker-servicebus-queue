"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function (orderAmount) {
    //var orderAmountElement = document.getElementById("order-amount");

    //var totalAmount = 0;
    //if (orderAmountElement.innerText) {
    //    totalAmount = parseInt(orderAmountElement.innerText, 10);
    //}

    showOrderQueue(orderAmount);
    //totalAmount = parseInt(orderAmount,10);
    //orderAmountElement.innerText = totalAmount;

    //var ordersConsumedElement = document.getElementById("orders-consumed");
    //var ordersConsumed = 0;
    //if (ordersConsumedElement.innerText) {
    //    ordersConsumed = parseInt(ordersConsumedElement.innerText, 10);
    //}
    //ordersConsumed += 1;
    //ordersConsumedElement.innerText = ordersConsumed;

});

function showOrderQueue(nrMessages) {
    console.log(nrMessages);

    var sample = [
        {
            language: '',
            value: nrMessages,
            color: '#000000'
        }];

    const svg = d3.select('svg');

    const margin = 80;
    const width = 1000 - 2 * margin;
    const height = 600 - 2 * margin;

    d3.selectAll("svg > *").remove();
    const chart = svg.append('g')
        .attr('transform', `translate(${margin}, ${margin})`);

    const xScale = d3.scaleBand()
        .range([0, width])
        .domain(sample.map((s) => s.language))
        .padding(0.4)

    const yScale = d3.scaleLinear()
        .range([height, 0])
        .domain([0, 100]);

    // vertical grid lines
    // const makeXLines = () => d3.axisBottom()
    //   .scale(xScale)

    const makeYLines = () => d3.axisLeft()
        .scale(yScale)

    chart.append('g')
        .attr('transform', `translate(0, ${height})`)
        .call(d3.axisBottom(xScale));

    chart.append('g')
        .call(d3.axisLeft(yScale));

    chart.append('g')
        .attr('class', 'grid')
        .call(makeYLines()
            .tickSize(-width, 0, 0)
            .tickFormat('')
        );

    const barGroups = chart.selectAll()
        .data(sample)
        .enter()
        .append('g');

    barGroups
        .append('rect')
        .attr('class', 'bar')
        .attr('x', (g) => xScale(g.language))
        .attr('y', (g) => yScale(g.value))
        .attr('height', (g) => height - yScale(g.value))
        .attr('width', xScale.bandwidth());
        
    barGroups
        .append('text')
        .attr('class', 'value')
        .attr('x', (a) => xScale(a.language) + xScale.bandwidth() / 2)
        .attr('y', (a) => yScale(a.value) + 30)
        .attr('text-anchor', 'middle');
        //.text((a) => `${a.value}`);

    svg
        .append('text')
        .attr('class', 'label')
        .attr('x', -(height / 2) - margin)
        .attr('y', margin / 2.4)
        .attr('transform', 'rotate(-90)')
        .attr('text-anchor', 'middle')
        .text('Nr of messages');

    svg.append('text')
        .attr('class', 'label')
        .attr('x', width / 2 + margin)
        .attr('y', height + margin * 1.7)
        .attr('text-anchor', 'middle')
        .text('Order Queue');

    svg.append('text')
        .attr('class', 'title')
        .attr('x', width / 2 + margin)
        .attr('y', 40)
        .attr('text-anchor', 'middle')
        .text('Order queue length');

}


connection.start().then(function() {
    showOrderQueue(50);

}).catch(function (err) {
    return console.error(err.toString());
});
