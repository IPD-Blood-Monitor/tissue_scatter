import tissue_scattering as sim
import sys, getopt

opts, args = getopt.getopt(sys.argv[1:], "w", "wavelength=")
wavelength = 660

for opt, arg in opts:
	if opt == "--wavelength":
		wavelength = arg

data = sim.scatter(int(wavelength), 0.1, 0.3, 0.05, 0.05, 1, 3, 0.150, 0.9)
print(data)

print(sim.calcAbsorptionCoefficient(940))
