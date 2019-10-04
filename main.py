import tissue_scattering as sim
import sys, getopt
import numpy as np

opts, args = getopt.getopt(sys.argv[1:], "w", "wavelength=")
wavelength = 660

for opt, arg in opts:
    if opt == "--wavelength":
        wavelength = arg

data = sim.scatter(int(wavelength), 0.1, 0.3, 0.05, 0.05, 1, 3, 0.150, 0.9)
print(data)

x = np.asarray([1, 0.1, 100, 1.5, 0.5, 0.3])
y = np.asarray([1.5, 0.3, 1.1, 0.7, 10, 0.17])
z = np.asarray([11, 0.7, 1.2, 1.7, 1.8, 0.2])

result = sim.filterPhotons(x, y, z, 1.0, 1.0)

print(result)
