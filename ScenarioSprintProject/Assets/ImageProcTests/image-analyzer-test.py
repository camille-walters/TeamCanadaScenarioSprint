# Source: https://pyimagesearch.com/2017/06/19/image-difference-with-opencv-and-python/
import cv2
import imutils
import socket
from skimage.metrics import structural_similarity as compare_ssim

UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

base = cv2.imread('Images/base.png')
flawed = cv2.imread('Images/0.png')

# Resize the flawed image to match the base
if flawed.shape != base.shape:
    flawed = cv2.resize(flawed, (base.shape[1], base.shape[0]))

grayA = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
grayB = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

(score, diff) = compare_ssim(grayA, grayB, full=True)
diff = (diff * 255).astype("uint8")

thresh = cv2.threshold(diff, 0, 255, cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
contours = cv2.findContours(thresh.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
contours = imutils.grab_contours(contours)

for c in contours:
    (x, y, w, h) = cv2.boundingRect(c)
    # cv2.rectangle(base, (x, y), (x + w, y + h), (0, 0, 255), 2)
    cv2.rectangle(flawed, (x, y), (x + w, y + h), (0, 0, 255), 2)

# cv2.imwrite('Images/boxOnOriginal.png', base)
cv2.imwrite('Images/boxOnFlawed.png', flawed)
cv2.imwrite('Images/diff.png', diff)
cv2.imwrite('Images/thresh.png', thresh)

print("SSIM: {}".format(score))
sock.sendto(("SSIM: {}".format(score)).encode(), (UDP_IP, UDP_PORT))
