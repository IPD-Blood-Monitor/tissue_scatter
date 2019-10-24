import tissue_scattering as sim
import sys, getopt
import numpy as np

import matplotlib.pyplot as plt

opts, args = getopt.getopt(sys.argv[1:], "w", "wavelength=")
wavelength = 660

for opt, arg in opts:
    if opt == "--wavelength":
        wavelength = arg

plot = [[], []]
print("running...")

for i in range(0, 100):
    data = sim.scatter(int(wavelength), 0.1, 0.3, 0.05, 0.05, 1, 3, 0.150, 0.9)
    plot[0].append(data[0])
    plot[1].append(data[1])
    print(".")

fig, axs = plt.subplots(2, sharex=True)

axs[0].plot(plot[0], label="Detector 1")
axs[1].plot(plot[1], label="Detector 2")
plt.xlabel("Samples")
plt.ylabel("Detected photons")

plt.legend()
plt.show()

data = sim.scatter(int(wavelength), 0.1, 0.3, 0.05, 0.05, 1, 3, 0.150, 0.9)
#print(data)

x = np.asarray([1, 0.1, 100, 1.1, 0.5, 0.3])
y = np.asarray([1.1, 0.3, 1.1, 0.7, 10, 0.17])
z = np.asarray([-11, -0.7, 1.2, -1.7, 1.8, 0.2])

result = sim.filterPhotons(x, y, z, 1.0, 1.0)

distance = np.asarray([2, 1, 2.3, 0.9, 0.1, 2.35])
amplitude = np.asarray([1, 1.1, 2, 0.7, 0.2, 1.8])

amp, lenghts = sim.detectorPhotons(x, y, z, distance, amplitude, 1, 0.5)

#print(result)
