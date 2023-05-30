import os
import cv2
import colour
import imutils
import socket
import numpy as np
from skimage.metrics import structural_similarity as compare_ssim

DIRECTORY = 'Images/Lighter/'


def flaw_analysis(base, flawed, image_number):
    # Source: https://pyimagesearch.com/2017/06/19/image-difference-with-opencv-and-python/
    gray_a = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
    gray_b = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

    (score, diff) = compare_ssim(gray_a, gray_b, full=True)
    diff = (diff * 255).astype("uint8")

    thresh = cv2.threshold(diff, 0, 255, cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
    contours = cv2.findContours(thresh.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    contours = imutils.grab_contours(contours)

    for c in contours:
        (x, y, w, h) = cv2.boundingRect(c)
        cv2.rectangle(flawed, (x, y), (x + w, y + h), (0, 0, 255), 2)
        '''
        # If the contour covers more than 20% of the image, there is a chance there is a color inconsistency
        if w * h > flawed.shape[0] * flawed.shape[1] * 0.2:
            print("Very large contour detected. Color consistency may be off")
        '''

    cv2.imwrite(f'{DIRECTORY}contours{image_number}.png', flawed)
    cv2.imwrite(f'{DIRECTORY}diff{image_number}.png', diff)
    cv2.imwrite(f'{DIRECTORY}thresh{i}.png', thresh)

    return score


def color_consistency_analysis(base, flawed, image_number):
    # Source: https://stackoverflow.com/a/57227800
    # Crop base and flawed to get just the colored cross-section of size (115, 590, 3)
    x = 95
    y = 160
    w = 590
    h = 115
    base = base[y:y + h, x:x + w]
    flawed = flawed[y:y + h, x:x + w]

    base = cv2.cvtColor(base.astype(np.float32) / 255, cv2.COLOR_RGB2Lab)
    flawed = cv2.cvtColor(flawed.astype(np.float32) / 255, cv2.COLOR_RGB2Lab)

    delta_e = colour.delta_E(base, flawed)
    delta_e = np.mean(delta_e)

    '''
    # cv2.imwrite(f'{DIRECTORY}crop{image_number}.png', flawed)
    # cv2.imwrite(f'{DIRECTORY}cropbase{image_number}.png', base)

    # gray_a = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
    # gray_b = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

    (score, diff) = compare_ssim(base[:, :, 0], flawed[:, :, 0], full=True)
    diff = (diff * 255).astype("uint8")
    print(score)
    # Checking for an overall color change
    rev = 255-diff
    gray_sum = np.sum(rev)
    score = gray_sum/(rev.shape[0]*rev.shape[1])
    
    cv2.imwrite(f'{DIRECTORY}rev{image_number}.png', diff)
    '''

    return delta_e


UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

prefixed = [filename for filename in os.listdir('Images') if filename.startswith("base") & filename.endswith(".png")]
total_images = len(prefixed)
total_image_for_color_analysis = total_images - 1
total_ssim_score = 0
total_color_score = 0

for i in range(0, total_images):
    base_image = cv2.imread(f'{DIRECTORY}base{i}.png')
    flawed_image = cv2.imread(f'{DIRECTORY}flawed{i}.png')

    # Resize the flawed image to match the base
    if flawed_image.shape != base_image.shape:
        flawed_image = cv2.resize(flawed_image, (base_image.shape[1], base_image.shape[0]))

    ssim_score = flaw_analysis(np.copy(base_image), np.copy(flawed_image), i)
    total_ssim_score += ssim_score

    # Do color analysis only for left and right view, not top
    if i != 1:
        color_change_metric = color_consistency_analysis(np.copy(base_image), np.copy(flawed_image), i)
        total_color_score += color_change_metric

print("SSIM: {}, Color inconsistency:{}".format(total_ssim_score / total_images, total_color_score / total_image_for_color_analysis))
sock.sendto(("SSIM: {}, Color inconsistency:{}".format(total_ssim_score / total_images, total_color_score / total_image_for_color_analysis)).encode(), (UDP_IP, UDP_PORT))
