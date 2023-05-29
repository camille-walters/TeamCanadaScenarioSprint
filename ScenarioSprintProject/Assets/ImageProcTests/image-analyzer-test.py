# Source: https://pyimagesearch.com/2017/06/19/image-difference-with-opencv-and-python/
import os
import cv2
import imutils
import socket
from skimage.metrics import structural_similarity as compare_ssim

UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

prefixed = [filename for filename in os.listdir('Images') if filename.startswith("base") & filename.endswith(".png")]
total_images = len(prefixed)
total_score = 0

for i in range(0, total_images):
    base = cv2.imread(f'Images/base{i}.png')
    flawed = cv2.imread(f'Images/flawed{i}.png')

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
        cv2.rectangle(flawed, (x, y), (x + w, y + h), (0, 0, 255), 2)

    cv2.imwrite(f'Images/contours{i}.png', flawed)
    cv2.imwrite(f'Images/diff{i}.png', diff)
    cv2.imwrite(f'Images/thresh{i}.png', thresh)

    total_score += score

print("SSIM: {}".format(total_score/total_images))
sock.sendto(("SSIM: {}".format(total_score/total_images)).encode(), (UDP_IP, UDP_PORT))
