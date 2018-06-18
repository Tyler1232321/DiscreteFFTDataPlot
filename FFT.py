import sys
import math
import time


#  This was used for debugging
def help(file, text):
    F = open(file,"w")
    F.write(text)
    F.close()

#   This read the data from the file and turned it into a complex array
def simple(filename):
    #   open file
    F = open(filename, "r")
    #   declare useful variables
    Time = []
    volts = []
    test = True
    Array = []
    #   Part that parses the data
    for line in F:
        try:
            line = float(line)
        except:
            test = False
            continue
        if test:
            Time.append(float(line))
        else:
            volts.append(float(line))
    #   Turns the data into a complex array
    for i in range(len(volts)):
        Array.append(complex(volts[i], 0))

    return Array

#   A couple complex arithmetic functions hard-coded
#region
def Add(a,b):
    ans = complex(a.real+b.real, a.imag+b.imag)
    return ans
def Sub(a,b):
    ans = complex(a.real-b.real, a.imag-b.imag)
    return ans
def Mul(a,b):
    ans = complex((a.real*b.real)-(a.imag*b.imag),(a.real*b.imag)+(a.imag*b.real))
    return ans
#endregion

#   The complex data structure
class complex:
    real = 0
    imag = 0
    def __init__(self,re,im):
        self.real = re
        self.imag = im
    def toString():
        ans = str(self.real) + " " + str(self.imag) + "i"
        return ans
    def phase(self):
        return math.Atan(self.imag/self.real)

    def mag(self):
        return math.sqrt(self.real**2+self.imag**2)
    def FromPolar(self, r, radians):
        ans = complex(r*math.cos(radians), r*math.sin(radians))
        return ans

#   Unused function that turns basic list into complex list
def MakeArrayComplex(List):
    for i in range(len(List)):
        List[i] = complex(float(List[i]),0)
    return List

#   Function that encaptulates the entire file, calls all of the other function to perform an FFT
def DoFFT(filename,filewrite):
    start_time = time.time()
    compArr = simple(filename)
    end_time = time.time()
    help("C:\\Python34\\ReadTime.txt", "It took " + str(start_time - end_time) +  " seconds to read the time domain data" )
    start_time = time.time()
    ans = FFT(compArr)
    ans = Magnitude(ans)
    end_time = time.time()
    dif = start_time-end_time
    help("C:\\Python34\\TransformTime.txt", "It took " + str(dif) + " seconds to perform the fourier transform")
    start_time= time.time()
    writeFile(ans,filewrite)
    end_time =time.time()
    dif = str(start_time-end_time)
    help("C:\\Python34\\WriteTime.txt", "It took " + dif + " seconds to write the Frequency domain data")
    return

#   Writes the data to a file
def writeFile(array, file):
    #help("is it getting here")
    f2 = open(file, "w")
    for i in range(len(array)):
        f2.write(str(array[i]) + "\n")
    f2.close()
    return

#   Performs the Fourier Transform given a complex list of data points
def FFT(compArr):
	N = len(compArr)
	Comp = [complex]*N
	d = []
	D = []
	e = []
	E = []

	if (N == 1):
		Comp[0] = compArr[0]
		return Comp

	for k in range(int(N/2)):
		e.append(compArr[2*k])
		d.append(compArr[2*k+1])

	D = FFT(d)
	E = FFT(e)

	for k in range(int(N/2)):
		temp = complex(0,0).FromPolar(1,-2*math.pi*k/N)
		D[k] = Mul(temp, D[k])

	for k in range(int(N/2)):
		Comp[k] = Add(E[k], D[k])
		Comp[k+int(N/2)] = Sub(E[k], D[k])

	return Comp

#   Gets the Magnitude from the Complex FFT results
def Magnitude(compArray):
    res = []
    for i in range(len(compArray)):
        res.append(compArray[i].mag())

    return res

#   Used for testing in the early stages
'''
Array = []
for i in range(len(volts)):
	Array.append(complex(volts[i], 0))
#Array = [complex(0,0),complex(0.707,0),complex(1,0),complex(0.707,0),complex(0,0),complex(-0.707,0),complex(-1,0),complex(-0.707,0)]
res = FFT(Array)
res = Magnitude(res)
f = open(args.destname, "w")
for i in range(len(res)):
	f.write(str(res[i]) + '\n')
'''