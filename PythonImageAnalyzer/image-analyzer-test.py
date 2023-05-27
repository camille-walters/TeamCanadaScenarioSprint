import numpy as np
import cv2


def mse(img1, img2):
    h, w = img1.shape
    diff = cv2.subtract(img1, img2)
    err = np.sum(diff**2)
    mse_error = err/(float(h*w))
    return mse_error, diff


base = cv2.imread('Screenshots/base.png')
flawed = cv2.imread('Screenshots/screenshot0.png')

'''
base = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
flawed = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

error, diff = mse(base, flawed)

print("Image matching Error between the two images:", error)

cv2.imwrite("Screenshots/diff.png", diff)

cv2.imshow("difference", diff)
cv2.waitKey(0)
cv2.destroyAllWindows()
'''

difference = cv2.subtract(base, flawed)

Conv_hsv_Gray = cv2.cvtColor(difference, cv2.COLOR_BGR2GRAY)
ret, mask = cv2.threshold(Conv_hsv_Gray, 0, 255,cv2.THRESH_BINARY_INV |cv2.THRESH_OTSU)
difference[mask != 255] = [0, 0, 255]

flawed[mask != 255] = [0, 0, 255]

cv2.imwrite('Screenshots/diffOverlay.png', flawed)
cv2.imwrite('Screenshots/diff.png', difference)
