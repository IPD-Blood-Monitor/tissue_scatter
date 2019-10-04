import numpy as np
from numpy.random import uniform, exponential

from numpy import pi

# the goal of this script is to calculate scattering through tissue, taking into accout
# normal cellular tissue, the presence of oxygenated blood, the presence of 
# deoxygenated blood, and the presence of bone
# to do this the following information must be acquired:
# scattering length for bone
# scattering length for tissue
# absorption coefficient for deoxygenated blood
# absorption coefficient for oxygenated blood


# the coode works by breaking the light beam up into a million particles, which are
# called photons for convienence. Each photon travels a length, l, that is drawn from 
# an exponential function that takes the scattering length into account
# the survival of the photon is drawn from a second exponential function that takes
# the absorption into account. If the photon survives, its direction is randomized
# and the process is repeated.

# Photons that exit the tissue at a detector location are kept, while photons that exit 
# elsewhere are dropped. New photons are continuously injected until the number of photons
# detected reaches sqrt(n)/n <= 2e-3 (e.g., the shot noise is 0.2% or less)

# the code should take in a minimal set of parameters:
# distance from tissue to bone (dTissue)
# concentration of blood in tissue
# ratio of oxygenated to deoxygenated blood
# locations of detectors (relative to light source which is always at 0,0,0)
#  


def obtainAbsorptionCoefficients(ld):
    #takes the wavelength and returns the absorption coefficient in cm^-1/M
    #, these still need to be converted into proper absorption coefficients 
    #(see the text file that contains the data
    # linear interpolation is used to obtain values between wavelengths in the file
    # wavelengths are in nm
    data = np.loadtxt("BloodAbsorptionData.txt", skiprows = 17)
    ldSpec = data[:,0]
    absOBloodArray = data[:,1]
    absBloodArray = data[:,2]
    absBlood = np.interp(ld, ldSpec, absBloodArray)
    absOBlood = np.interp(ld, ldSpec, absOBloodArray)
    return absBlood, absOBlood
    

def obtainScatteringCoefficients(ld):
    #takes the wavelength and returns the scattering coefficient
    #this uses an equation for the reduced scattering coefficient mu = a(ld/ld_ref)^-b
    #a and b are tissue-dependent coefficients (but are wavelength independent)
    #ld_ref = 500 nm
    #we use constants obtained from Phys. Med. Biol. 58(2013) R37-R61
    #we use the mean values for skin and bone and a specific value for muscle
    #(from Tromber 1996 in the referenced paper)
    #ld: wavelength in nanometers
    #returns three scattering coefficients in cm^-1
    
    aSkin = 46.0
    aBone = 22.9
    aMuscle = 13.0
    bSkin = -1.421
    bBone = -0.716
    bMuscle = -1.470
    ldRef = 500
    
    muSkin = aSkin*(ld/ldRef)**bSkin
    muBone = aBone*(ld/ldRef)**bBone
    muMuscle = aMuscle*(ld/ldRef)**bMuscle
    return muSkin, muBone, muMuscle
    
def calcAbsorptionCoefficient(absBlood, absOBlood, concBlood, ratio):
    #calculates the absorption coefficient based on the hemoglobin 1/(cm.Mol/l) molar absorption coefficients
    #We use the concentration (mg/l) of hemoglobin and the ratio to determine the amount of
    #oxygenated and deoxygenated hemoglobin in the volume.
    # 64500 is the number of mg/Mol for hemoglobin
    cO = concBlood*ratio #mg/l of oxygenated hemoglobin
    cNoO = concBlood*(1-ratio) #mg/l of deoxygenated hemoglobin
    absO = cO*absOBlood/64500.0 #/cm absorption coefficient ((1/(cm.mol/l)*mg/l/mg = 1/cm)) oxygenated hemoglobin
    absNoO = cNoO*absBlood/64500.0 #/cm absorption coefficient ((1/(cm.mol/l)*mg/l/mg = 1/cm)) deoxygenated hemoglobin
    return absO + absNoO # we can add them because for two components, beers law looks like this: 
    #I = I0*exp(-alpha1*l)exp(-alpha2*l) = I0*exp(-(alpha1 + alpha2)*l)
    
    
def filterPhotons(x, y, z, xBound, zBound):
    #no need to keep doing calculations for photons that are out of the boundary of the calculation
    #we find the indexes of those photons then return the indexes and the total number of photons
    #left in the volume
    inXBounds = np.where(np.abs(x) <= xBound, 1, 0)
    inYBounds = np.where(np.abs(y) <= xBound, 1, 0)
    inZBoundsDeep = np.where(z <= zBound, 1, 0)
    inZBoundsSkin = np.where(z >= 0, 1, 0)
    inBounds = inXBounds*inYBounds*inZBoundsDeep*inZBoundsSkin
    idxInModel = np.nonzero(inBounds)[0]
    numPhotons = inBounds.sum()
    return idxInModel, numPhotons
    
def detectorPhotons(x, y, z, dist, amp, dDet, width):
    #detector is a ring around the source
    # find all photons that are within the ring at z<0 (which means they have left the skin and can't scatter back)
    rPos = np.sqrt(x**2 + y**2)
    rBounds = np.where(rPos > (dDet - width), 1, 0)*np.where(rPos <= (dDet + width), 1, 0)
    zBounds = np.where(z < 0, 1, 0)
    inBounds = rBounds*zBounds
    photonAmplitude = (inBounds*amp).sum()
    lengths = dist[np.nonzero(inBounds)[0]]
    return photonAmplitude, lengths
    
def updatePositions(x, y, z, dist, idx, amp, numPhotons, mu, absCoef):
    #update positions. Use exponential distribution to generate distances (based on scattering length)
    # theta and phi are randomly generated angles (uniform distribution) to give direction in 3D
    # Use coordinate transform to track cartesian coordinate location of photons
    distance = exponential(1/mu, numPhotons)
    dist[idx] += distance
    theta = uniform(-pi/2, pi/2, numPhotons)
    phi = uniform(0, 2*pi, numPhotons)
    x[idx] = distance*np.cos(theta)*np.cos(phi)
    y[idx] = distance*np.cos(theta)*np.cos(phi)
    z[idx] = distance*np.sin(theta)
    amp[idx] = amp[idx]*np.exp(-absCoef*distance)
    return x, y, z, amp, dist
    
def areWeDone(det1, det2):
    # determine if the signal to noise is good enough
    # require that A/sqrt(A) < thresh for both detectors
    thresh = 0.01
    if det1 == 0:
        return True
    elif det2 == 0:
        return True
    elif np.sqrt(det1)/det1 > thresh:
        #print("detector 1: ", np.sqrt(det1)/det1)
        return True
    elif np.sqrt(det2)/det2 > thresh:
        #print("detector 2 ", np.sqrt(det2)/det2)
        return True
    else:
        return False
    
def scatter(ld, dDet1, dDet2, width, dSkin, dMuscle, dBone, concBlood, ratioOxygen):
    #calculates scattering through tissue for a specific wavelength
    #Takes in:
    #ld = wavelength (nanometers)
    #dDet1 = distance to first detector (in cm)
    #dDet2 = distance to second detector (in cm) dDet2 > dDet 2
    #width is the half width of both the light source and the detectors (cm) 
    #dSkin = thickness of skin in cm
    #dMuscle = thickness of muscle tissue
    #dBone = thickness of bone. Note photons that go through the bone are dropped
    #concBlood = concentration of hemoglobin ~150g/liter 
    #ratioOxygen = ratio of oxygenated to deoxygenated hemoglobin (0-1)
    
    #we place the two detectors as a ring around the light source 
    # and bound the model volume by 5*dDet2
    
    # for absorption, each photon is given a starting weight, which is reduced
    # by absorption
    xBound = 5*dDet2
    yBound = 5*dDet2
    zBound = dSkin + dMuscle + dBone
    
    muSkin, muBone, muMuscle = obtainScatteringCoefficients(ld)
    absBlood, absOBlood = obtainAbsorptionCoefficients(ld)
    absCoef = calcAbsorptionCoefficient(absBlood, absOBlood, concBlood, ratioOxygen)
    #print (absCoef)
    #assume the areas of the light emitter is 0.1 cm square and that contact with the skin is
    # not perfect
    numPhotons = int(5e6)
    xPos = uniform(-width, width, numPhotons)
    yPos = uniform(-width, width, numPhotons)
    zPos = uniform(0, 1/muSkin, numPhotons)
    distance = np.zeros((numPhotons,), np.float)
    amplitude = 100*np.ones(xPos.shape, np.float)
    keepScattering = True
    idxInModel = np.arange(numPhotons, dtype = np.int32)
    numDet1Photons = 0
    numDet2Photons = 0
    firstD1 = True
    firstD2 = True
    while keepScattering:
        #remove all photons outside the model boundaries from the calculation
        xPos = xPos[idxInModel]
        yPos = yPos[idxInModel]
        zPos = zPos[idxInModel]
        distance = distance[idxInModel]
        
        #Determine which photons are in which tissue and their indexes
        amplitude = amplitude[idxInModel]
        inSkin = np.where(zPos <= dSkin, 1, 0)
        idxSkin = np.nonzero(inSkin)[0]
        numInSkin = inSkin.sum()
        
        inMuscle = np.where(zPos <= dMuscle, 1, 0)*np.where(zPos > dSkin, 1, 0)
        idxMuscle = np.nonzero(inMuscle)[0]
        numInMuscle = inMuscle.sum()
        
        inBone = np.where(zPos <= dBone, 1, 0)*np.where(zPos > dSkin + dMuscle, 1, 0)
        idxBone = np.nonzero(inBone)[0]
        numInBone = inBone.sum()

        
        
        #determine the distance each photon travels using the right scattering coefficient
        #then use angles and that length to update photon positions
        if numInSkin > 0:
            xPos, yPos, zPos, amplitude, distance = updatePositions(xPos, yPos, zPos, distance, idxSkin, amplitude, numInSkin, muSkin, 0)
            
        #we assume that the vast majority of blood is in the muscle not the skin or bone, hence the abs coefficient is 0
        if numInMuscle > 0:
            xPos, yPos, zPos, amplitude, distance = updatePositions(xPos, yPos, zPos, distance, idxMuscle, amplitude, numInMuscle, muMuscle, absCoef)
            #print (temp, numInBone)
        if numInBone > 0:
            xPos, yPos, zPos, amplitude, distance = updatePositions(xPos, yPos, zPos, distance, idxBone, amplitude, numInBone, muBone, 0)
            #as with skin, we assume no blood in the bone (not true if you include marrow) ==> abs coef is 0
        #need to remove photons that are outside the calculation bounds and count up those that are
        #detected
        idxInModel, numPhotons = filterPhotons(xPos, yPos, zPos, xBound, zBound)
        
        #calculate the intensity at each detector
        arrivedPhotons, lengths1 = detectorPhotons(xPos, yPos, zPos, distance, amplitude, dDet1, width)
        numDet1Photons += arrivedPhotons
        arrivedPhotons, lengths2 = detectorPhotons(xPos, yPos, zPos, distance, amplitude, dDet2, width)
        numDet2Photons += arrivedPhotons
        if firstD1:
            lengthToD1 = lengths1
            firstD1 = False
        else:
            lengthToD1 = np.concatenate((lengthToD1, lengths1))
            
        if firstD2:
            lengthToD2 = lengths2
            firstD2 = False
        else:
            lengthToD2 = np.concatenate((lengthToD2, lengths2))
        
        # check to see if we have sufficient signal-to-noise at both detectors
        keepScattering = areWeDone(numDet1Photons, numDet2Photons)
        #make sure the calculation is not doing anything stupid
        #print(numDet1Photons, numDet2Photons, numPhotons)
        if numPhotons < 100000:
            #if we run out of photons before signal-to-noise is above threshold, inject some more 
            xPos = np.concatenate((xPos, uniform(-width, width, 5000000)))
            yPos = np.concatenate((yPos, uniform(-width, width, 5000000)))
            zPos = np.concatenate((zPos, uniform(0, 1/muSkin, 5000000)))
            amplitude = np.concatenate((amplitude, np.ones((5000000,), np.float)))
            idxInModel, numPhotons = filterPhotons(xPos, yPos, zPos, xBound, zBound)
            
    return numDet1Photons, numDet2Photons, lengthToD1.mean(), lengthToD2.mean()
        
    
def scatteringSpectrum(ldStart, ldEnd, numSteps, conc, ratio):
    #this function will calculate the detector amplitudes for a fixed concentration and ratio
    #but scan over the wavelengths
    ldArray = np.linspace(ldStart, ldEnd, numSteps)
    ampDet1 = np.zeros(ldArray.shape)
    ampDet2 = np.zeros(ldArray.shape)
    l1 = np.zeros(ldArray.shape)
    l2 = np.zeros(ldArray.shape)
    for idx in range(max(ldArray.shape)):
        #print (ldArray[idx])
        ampDet1[idx], ampDet2[idx], l1[idx], l2[idx] = scatter(ldArray[idx], 0.1, 0.3, 0.05, 0.01, 1, 3, conc, ratio)
    return ldArray, ampDet1, ampDet2, l1, l2
    
    
def scatteringOxygenRatioVariation(ratioStart, ratioEnd, numSteps, ld, conc):
    #this function will calculate the detector amplitudes for a fixed concentration and wavelength
    #but scan over oxygenation ratios
    ratioArray = np.linspace(ratioStart, ratioEnd, numSteps)
    ampDet1 = np.zeros(ratioArray.shape)
    ampDet2 = np.zeros(ratioArray.shape)
    l1 = np.zeros(ratioArray.shape)
    l2 = np.zeros(ratioArray.shape)
    for idx in range(max(ratioArray.shape)):
        print (ratioArray[idx])
        ampDet1[idx], ampDet2[idx], l1[idx], l2[idx] = scatter(ld, 0.1, 0.3, 0.05, 0.05, 1, 3, conc, ratioArray[idx])
    return ratioArray, ampDet1, ampDet2, l1, l2
    
def scatteringBloodConcentrationVariation(concStart, concEnd, numSteps, ld, ratio):
    #this function will calculate the detector amplitudes for a fixed oxygenation ratio and wavelength
    #but scan over blood concentration
    concArray = np.linspace(concStart, concEnd, numSteps)
    ampDet1 = np.zeros(concArray.shape)
    ampDet2 = np.zeros(concArray.shape)
    l1 = np.zeros(concArray.shape)
    l2 = np.zeros(concArray.shape)
    for idx in range(max(concArray.shape)):
        #print (concArray[idx])
        ampDet1[idx], ampDet2[idx], l1[idx], l2[idx] = scatter(ld, 0.1, 0.3, 0.05, 0.05, 1, 3, concArray[idx], ratio)
    return concArray, ampDet1, ampDet2, l1, l2

def scanAllParameters(ldStart, ldEnd, numldSteps, concStart, concEnd, numConcSteps, ratioStart, ratioEnd, numRatioSteps):
    ldArray = np.linspace(ldStart, ldEnd, numldSteps)
    concArray = np.linspace(concStart, concEnd, numConcSteps)
    data = np.zeros((numldSteps, numConcSteps, numRatioSteps, 7))
    for ldIdx in range(numldSteps):
        for concIdx in range(numConcSteps):
            print(ldArray[ldIdx], concArray[concIdx], ratioStart, ratioEnd)
            data[ldIdx, concIdx,:,0] = ldArray[ldIdx]
            data[ldIdx, concIdx,:,1] = concArray[ldIdx]
            cArr, d1Arr, d2Arr, l1, l2 = scatteringOxygenRatioVariation(ratioStart, ratioEnd, numRatioSteps, ldArray[ldIdx], concArray[concIdx])
            data[ldIdx, concIdx, :, 2] = cArr
            data[ldIdx, concIdx, :, 3] = d1Arr
            data[ldIdx, concIdx, :, 4] = d2Arr
            data[ldIdx, concIdx, :, 5] = l1
            data[ldIdx, concIdx, :, 6] = l2
    return data
    
def calcAbsDifference(amp1, amp2):
    R11 = amp1[0]/amp2[0]
    R22 = amp1[1]/amp2[1]
    R33 = amp1[2]/amp2[2]
    print (R11, R22, R33)
    R12 = R11/R22
    R13 = R11/R33
    print (R12, R13)
    dalpha12 = np.log(R12)
    dalpha13 = np.log(R13)
    print (dalpha12, dalpha13)
    return dalpha12, dalpha13
    
def calcConcentration(alpha12, alpha13, ld):
    alphaD1, alphaO1 = obtainAbsorptionCoefficients(ld[0])
    alphaD2, alphaO2 = obtainAbsorptionCoefficients(ld[1])
    alphaD3, alphaO3 = obtainAbsorptionCoefficients(ld[2])
    topLine = alpha12*(alphaD3 + alphaD1) - alpha13*(alphaD1 + alphaD2)
    botLine = alpha13*(alphaO1 - alphaO2 + alphaD1 - alphaD2) - alpha12*(alphaO1 + alphaO3 - alphaD1 - alphaD3)
    ratio = np.sqrt((topLine/botLine)**2)
    if ratio > 1:
        print ("ratio is invalid", ratio)
    elif ratio < 0:
        print ("ratio is invalid", ratio)
    conc = (1/alpha12)*(alphaO2/ratio + alphaD1/(1-ratio) - alphaO3/ratio + alphaD3/(1 - ratio))
    return ratio, conc
    

#scatterData = scanAllParameters(400, 900, 3, 0.125, 12.5, 3, 0, 1, 3)
ldArr, d1, d2, l1, l2 = scatteringSpectrum(400, 900, 3, 1.25, 0.1)

a1, a2 = calcAbsDifference(d1, d2)

ratio, conc = calcConcentration(a1, a2, ldArr)
print (ratio, l1, l2, l2-l1)

#print(scatteringSpectrum(400, 900, 3, 1.25, 0.5))

#print(scatteringOxygenRatioVariation(0, 1, 3, 525, 0.125))

#print(scatteringBloodConcentrationVariation(0.0125, 1.25, 3, 525, 0.5))
