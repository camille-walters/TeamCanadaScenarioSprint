# User will pass files through command line
import csv
import sys
import plotly.express as px
import plotly.graph_objects as go
from plotly.subplots import make_subplots
from datetime import datetime

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
                    # sim[i].append(split[i])
                    sim_time = datetime.strptime(split[i], '%H:%M:%S')  # .time()
                    sim[i].append(sim_time)
                else:
                    sim[i].append(float(split[i]))

    # switch from time to time intervals, needed for overlay graphs
    base_time = sim[0][0]
    times = sim[0]
    #sim[0] = [t for t in times]
    index = 0
    for t in times:
        times[index] = t-base_time
        times[index] = times[index].seconds
        index += 1
    sim[0] = times

    # add final row with number of defects per car
    defects_per_car = [x/y if y else 0 for x, y in zip(sim[12], sim[16])]
    sim.append(defects_per_car)
    '''
    defects_per_car = sim[12]
    for x in range(0, len(defects_per_car)):
        if sim[16][i] != 0:
            defects_per_car[i] = defects_per_car[i]/sim[16][i]
        else:
            defects_per_car[i] =0
    sim.append(defects_per_car)
    '''
    print(defects_per_car)


    sims.append(sim)

# Get minimum number of values for uniformity across graphs
min_array = []
for i in range(0, len(sims)):
    min_array.append(len(sims[i][0]))
count = min(min_array)
print(count)

'''
# working, one under the other 
fig = make_subplots(rows=len(sims[0][0]), cols=1)
for i in range(0, len(sims)):
    trace = go.Line(x=sims[i][0][:count], y=sims[i][15][:count], name="Simulation"+str(i))
    fig.add_trace(trace, row=i+1, col=1)

fig.update_layout(height=8000, title_text="Worker Utilization")
fig.write_html('sims.html', auto_open=True)
'''

# working, overlay graphs
fig_total_cars_processed = make_subplots(specs=[[{"secondary_y": True}]])
fig_time_to_process_car = make_subplots(specs=[[{"secondary_y": True}]])
fig_defects = make_subplots(specs=[[{"secondary_y": True}]])
fig_defects_per_car = make_subplots(specs=[[{"secondary_y": True}]])
fig_worker_utilization = make_subplots(specs=[[{"secondary_y": True}]])

for i in range(0, len(sims)):
    trace_total_cars_processed = go.Line(x=sims[i][0][:count], y=sims[i][16][:count], name="Simulation" + str(i))
    trace_total_time_to_process_cars = go.Line(x=sims[i][0][:count], y=sims[i][14][:count], name="Simulation" + str(i))
    trace_total_defects = go.Line(x=sims[i][0][:count], y=sims[i][12][:count], name="Simulation" + str(i))
    trace_total_defects_per_car = go.Line(x=sims[i][0][:count], y=sims[i][17][:count], name="Simulation" + str(i))
    trace_worker_utilization = go.Line(x=sims[i][0][:count], y=sims[i][15][:count], name="Simulation" + str(i))

    if i == 0:
        fig_total_cars_processed.add_trace(trace_total_cars_processed)
        fig_time_to_process_car.add_trace(trace_total_time_to_process_cars)
        fig_defects.add_trace(trace_total_defects)
        fig_defects_per_car.add_trace(trace_total_defects_per_car)
        fig_worker_utilization.add_trace(trace_worker_utilization)
    else:
        fig_total_cars_processed.add_trace(trace_total_cars_processed)  #, secondary_y=True)
        fig_time_to_process_car.add_trace(trace_total_time_to_process_cars)  #, secondary_y=True)
        fig_defects.add_trace(trace_total_defects)  #, secondary_y=True)
        fig_defects_per_car.add_trace(trace_total_defects_per_car)  #, secondary_y=True)
        fig_worker_utilization.add_trace(trace_worker_utilization)  #, secondary_y=True)

fig_total_cars_processed.update_layout(yaxis_title="Number of cars processed", xaxis_title="Time (seconds)", title_text="Total Cars Processed")
fig_total_cars_processed.write_html('total_cars_processed.html', auto_open=True)

'''
fig_time_to_process_car.update_layout(yaxis_title="Time to process each car", xaxis_title="Time (seconds)", title_text="Time to process each car")
fig_time_to_process_car.write_html('time_to_process_car.html', auto_open=True)
'''

fig_defects.update_layout(yaxis_title="Number of defects", xaxis_title="Time (seconds)", title_text="Total Defects")
fig_defects.write_html('total_defects.html', auto_open=True)

fig_defects_per_car.update_layout(yaxis_title="Number of defects per car", xaxis_title="Time (seconds)", title_text="Defects per Car")
fig_defects_per_car.write_html('total_defects_per_car.html', auto_open=True)

fig_worker_utilization.update_layout(yaxis_title="Worker utilization", xaxis_title="Time (seconds)", title_text="Worker Utilization")
fig_worker_utilization.write_html('worker_utilization.html', auto_open=True)
