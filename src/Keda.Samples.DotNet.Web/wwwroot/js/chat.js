"use strict";

window.setInterval(function () {

    showOrderQueue(50);

}, 2000);

function showOrderQueue(nrMessages) {

    fetch("/api/v1/queue/")
        .then(function (response) {

            if (response.status !== 200) {
                console.log('Looks like there was a problem. Status Code: ' +
                    response.status);
                return;
            }

            response.json().then(function (data) {
                console.log(data);
                var measure = [
                    {
                        language: '',
                        value: data.messageCount,
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
                    .domain(measure.map((s) => s.language))
                    .padding(0.4);

                const yScale = d3.scaleLinear()
                    .range([height, 0])
                    .domain([0, 500]);

                // vertical grid lines
                // const makeXLines = () => d3.axisBottom()
                //   .scale(xScale)

                const makeYLines = () => d3.axisLeft()
                    .scale(yScale);

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
                    .data(measure)
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
            });
        });


}

