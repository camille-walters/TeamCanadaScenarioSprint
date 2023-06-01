import os
import time

import cv2
import colour
import imutils
import socket
import numpy as np
from skimage.metrics import structural_similarity as compare_ssim

DIRECTORY = '../CVCaptures/'
SAVE_DIRECTORY = '../CVCaptures/Contours/'


def flaw_analysis(base, flawed, image_number, car_number):
    # Source: https://pyimagesearch.com/2017/06/19/image-difference-with-opencv-and-python/
    gray_a = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
    gray_b = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

    gray_a = cv2.GaussianBlur(gray_a, (3, 3), sigmaX=0, sigmaY=0)
    gray_b = cv2.GaussianBlur(gray_b, (3, 3), sigmaX=0, sigmaY=0)

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

    cv2.imwrite(f'{SAVE_DIRECTORY}contours{car_number}_{image_number}.png', flawed)
    # cv2.imwrite(f'{DIRECTORY}diff{image_number}.png', diff)
    # cv2.imwrite(f'{DIRECTORY}thresh{car_number}_{image_number}.png', thresh)

    return score


def color_consistency_analysis(base, flawed):
    # Source: https://stackoverflow.com/a/57227800
    # TODO: Maybe switch this to go over specific pixels with differences and highlight those?

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

    return delta_e


def edge_detection(base, flawed, image_number, car_number):
    # Source: https://learnopencv.com/edge-detection-using-opencv/
    base_gray = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
    flawed_gray = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

    base_blur = cv2.GaussianBlur(base_gray, (3, 3), sigmaX=0, sigmaY=0)
    flawed_blur = cv2.GaussianBlur(flawed_gray, (3, 3), sigmaX=0, sigmaY=0)
    # Sobel Edge Detection
    base_sobel_xy = cv2.Sobel(src=base_blur, ddepth=cv2.CV_64F, dx=1, dy=1, ksize=5)  # Combined X and Y Sobel
    sobel_xy = cv2.Sobel(src=flawed_blur, ddepth=cv2.CV_64F, dx=1, dy=1, ksize=5)  # Combined X and Y Sobel
    '''
    # Canny Edge Detection
    base_edges = cv2.Canny(image=base_blur, threshold1=100, threshold2=200)
    edges = cv2.Canny(image=flawed_blur, threshold1=100, threshold2=200)
    '''

    (score, diff) = compare_ssim(base_sobel_xy, sobel_xy, data_range=sobel_xy.max() - sobel_xy.min(), full=True)
    diff = (diff * 255).astype("uint8")

    thresh = cv2.threshold(diff, 0, 255, cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
    contours = cv2.findContours(thresh.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    contours = imutils.grab_contours(contours)

    for c in contours:
        (x, y, w, h) = cv2.boundingRect(c)
        cv2.rectangle(flawed, (x, y), (x + w, y + h), (0, 0, 255), 2)

    # print(len(contours))
    cv2.imwrite(f'{DIRECTORY}edge_contours{car_number}_{image_number}.png', flawed)
    cv2.imwrite(f'{DIRECTORY}edge_diff{car_number}_{image_number}.png', diff)

    return score


def analyze_one_car(car_number):
    total_images = 3
    total_image_for_color_analysis = total_images - 1
    total_ssim_score = 0
    total_color_score = 0

    for i in range(0, total_images):
        base_image = cv2.imread(f'{DIRECTORY}base_{i}.png')
        flawed_image = cv2.imread(f'{DIRECTORY}flawed{car_number}_{i}.png')

        # Resize the flawed image to match the base
        if flawed_image.shape != base_image.shape:
            flawed_image = cv2.resize(flawed_image, (base_image.shape[1], base_image.shape[0]))

        ssim_score = flaw_analysis(np.copy(base_image), np.copy(flawed_image), i, car_number)
        total_ssim_score += ssim_score

        # Do color analysis only for left and right view, not top
        if i != 1:
            color_change_metric = color_consistency_analysis(np.copy(base_image), np.copy(flawed_image))
            total_color_score += color_change_metric

        # edge_detection(np.copy(base_image), np.copy(flawed_image), i, car_number)

    print("SSIM: {}, Color inconsistency:{}".format(total_ssim_score / total_images,
                                                    total_color_score / total_image_for_color_analysis))
    sock.sendto(("SSIM: {}, Color inconsistency:{}".format(total_ssim_score / total_images,
                                                           total_color_score / total_image_for_color_analysis)).encode(),
                (UDP_IP, UDP_PORT))


UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

prev_number_of_flawed_images = 0
number_of_flawed_images = 0
car_counter = 0

try:
    while True:
        number_of_flawed_images = len([filename for filename in os.listdir(DIRECTORY) if
                                   filename.startswith("flawed") & filename.endswith(".png")])

        if number_of_flawed_images != prev_number_of_flawed_images:
            # 3 new images incoming
            print(f'{number_of_flawed_images} and {prev_number_of_flawed_images}')
            time.sleep(0.3)
            analyze_one_car(car_counter)
            car_counter += 1
            number_of_flawed_images = prev_number_of_flawed_images + 3

        prev_number_of_flawed_images = number_of_flawed_images

except KeyboardInterrupt:
    pass
