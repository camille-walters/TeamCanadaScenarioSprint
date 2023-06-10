# User will pass files through command line
import csv
import sys
import plotly.express as px
import plotly.graph_objects as go
from plotly.subplots import make_subplots

files = []
sims = []

for i in range(1, len(sys.argv)):
    files.append(sys.argv[i])

for f in files:
    counter = 0
    sim = [[], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], []]
    with open(f, newline='') as csvfile:
        reader = csv.reader(csvfile, delimiter=' ', quotechar='|')
        for row in reader:
            if counter == 0:
                counter += 1
                continue
            split = row[0].split(",")

            for i in range(0, len(split)):
                if i == 0:
                    # time
                    sim[i].append(split[i])
                else:
                    sim[i].append(float(split[i]))
    # print(sim)
    sims.append(sim)
'''
fig = make_subplots(specs=[[{"secondary_y": True}]])
for i in range(0, len(sims)):
    # fig = px.line(x=sims[i][0], y=sims[i][15])
    trace = go.Line(x=sims[i][0], y=sims[i][15])
    fig.add_trace(trace)

fig.write_html('sims.html', auto_open=True)
'''
print(len(sims))

# Get minimum number of values for uniformity across graphs
min_array = []
for i in range(0, len(sims)):
    min_array.append(len(sims[i][0]))
count = min(min_array)
print(count)

fig = make_subplots(rows=len(sims[0][0]), cols=1)
for i in range(0, len(sims)):
    trace = go.Line(x=sims[i][0][:count], y=sims[i][15][:count], name="Simulation"+str(i))
    fig.add_trace(trace, row=i+1, col=1)

fig.update_layout(height=8000, title_text="Worker Utilization")
fig.write_html('sims.html', auto_open=True)
