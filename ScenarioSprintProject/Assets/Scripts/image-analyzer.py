import os
import time
import cv2
import colour
import imutils
import socket
import glob
import numpy as np
from imutils import contours
from skimage.metrics import structural_similarity as compare_ssim

DIRECTORY = '../CVCaptures/'
SAVE_DIRECTORY = '../CVCaptures/Contours/'


def merge_overlapping_boxes(boxes):
    boxes_used = [False] * len(boxes)
    accepted_boxes = []

    for supIdx, supVal in enumerate(boxes):
        if not boxes_used[supIdx]:
            curr_x_min = supVal[0]
            curr_x_max = supVal[0] + supVal[2]
            curr_y_min = supVal[1]
            curr_y_max = supVal[1] + supVal[3]
            boxes_used[supIdx] = True

            for subIdx, subVal in enumerate(boxes[(supIdx + 1):], start=(supIdx + 1)):

                # Initialize merge candidate
                candidate_x_min = subVal[0]
                candidate_x_max = subVal[0] + subVal[2]
                candidate_y_min = subVal[1]
                candidate_y_max = subVal[1] + subVal[3]

                if (curr_x_max > candidate_x_max) and (curr_x_min < candidate_x_min) and (
                        curr_y_max > candidate_y_max) and (curr_y_min < candidate_y_min):
                    boxes_used[subIdx] = True

            accepted_boxes.append([curr_x_min, curr_y_min, curr_x_max - curr_x_min, curr_y_max - curr_y_min])

    return accepted_boxes


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
    (contours, bounding_boxes) = imutils.contours.sort_contours(contours, method="left-to-right")
    bounding_boxes = list(bounding_boxes)
    bounding_boxes = merge_overlapping_boxes(bounding_boxes)
    # bounding_boxes = cv2.groupRectangles(bounding_boxes, 1, 0.01)

    threshold = 1000
    ignore_threshold = 20
    minor_counter = 0
    major_counter = 0

    for (x, y, w, h) in bounding_boxes:
        if w * h > threshold:
            major_counter += 1
            cv2.rectangle(flawed, (x, y), (x + w, y + h), (0, 0, 255), 2)  # Red
        elif w * h > ignore_threshold:
            minor_counter += 1
            cv2.rectangle(flawed, (x, y), (x + w, y + h), (0, 255, 0), 2)  # Green
        else:
            continue  # ignore

    cv2.imwrite(f'{SAVE_DIRECTORY}contours{car_number}_{image_number}.png', flawed)
    # cv2.imwrite(f'{DIRECTORY}diff{image_number}.png', diff)
    # cv2.imwrite(f'{DIRECTORY}thresh{car_number}_{image_number}.png', thresh)

    return score, minor_counter, major_counter


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


'''
def edge_detection(base, flawed, image_number, car_number):
    # Source: https://learnopencv.com/edge-detection-using-opencv/
    base_gray = cv2.cvtColor(base, cv2.COLOR_BGR2GRAY)
    flawed_gray = cv2.cvtColor(flawed, cv2.COLOR_BGR2GRAY)

    base_blur = cv2.GaussianBlur(base_gray, (3, 3), sigmaX=0, sigmaY=0)
    flawed_blur = cv2.GaussianBlur(flawed_gray, (3, 3), sigmaX=0, sigmaY=0)
    
    # Sobel Edge Detection
    base_sobel_xy = cv2.Sobel(src=base_blur, ddepth=cv2.CV_64F, dx=1, dy=1, ksize=5)  # Combined X and Y Sobel
    sobel_xy = cv2.Sobel(src=flawed_blur, ddepth=cv2.CV_64F, dx=1, dy=1, ksize=5)  # Combined X and Y Sobel
    
    # Canny Edge Detection
    base_edges = cv2.Canny(image=base_blur, threshold1=100, threshold2=200)
    edges = cv2.Canny(image=flawed_blur, threshold1=100, threshold2=200)

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
'''


def analyze_one_car(car_number):
    total_images = 3
    total_image_for_color_analysis = total_images - 1
    total_ssim_score = 0
    total_minor_flaws = 0
    total_major_flaws = 0
    total_color_score = 0

    for i in range(0, total_images):
        base_image = cv2.imread(f'{DIRECTORY}base_{i}.png')
        flawed_image = cv2.imread(f'{DIRECTORY}flawed{car_number}_{i}.png')

        # Resize the flawed image to match the base
        if flawed_image.shape != base_image.shape:
            flawed_image = cv2.resize(flawed_image, (base_image.shape[1], base_image.shape[0]))

        ssim_score, minor_flaws, major_flaws = flaw_analysis(np.copy(base_image), np.copy(flawed_image), i, car_number)
        total_ssim_score += ssim_score
        total_minor_flaws += minor_flaws
        total_major_flaws += major_flaws

        # Do color analysis only for left and right view, not top
        if i != 1:
            color_change_metric = color_consistency_analysis(np.copy(base_image), np.copy(flawed_image))
            total_color_score += color_change_metric

    print("SSIM: {:.3f}, Minor flaws: {}, Major flaws: {}, Color inconsistency:{:.3f}".format(total_ssim_score / total_images,
                                                                                      total_minor_flaws,
                                                                                      total_major_flaws,
                                                                                      total_color_score / total_image_for_color_analysis))
    sock.sendto(
        ("SSIM: {:.3f}, Minor flaws: {}, Major flaws: {}, Color inconsistency:{:.3f}".format(total_ssim_score / total_images,
                                                                                     total_minor_flaws,
                                                                                     total_major_flaws,
                                                                                     total_color_score / total_image_for_color_analysis)).encode(),
        (UDP_IP, UDP_PORT))


UDP_IP = "127.0.0.1"
UDP_PORT = 5065

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

prev_number_of_flawed_images = 0
number_of_flawed_images = 0
car_counter = 0

# Clean up folders before analysis
# Clean Contours folder
filelist = glob.glob(os.path.join(SAVE_DIRECTORY, "*"))
for f in filelist:
    os.remove(f)
# Clean CVCaptures folder, but leave base images
filelist = glob.glob(os.path.join(DIRECTORY, "*"))
for f in filelist:
    if f.__contains__("flawed"):
        os.remove(f)
print("Cleaned folders, ready to start analysis")

try:
    while True:
        number_of_flawed_images = len([filename for filename in os.listdir(DIRECTORY) if
                                       filename.startswith("flawed") & filename.endswith(".png")])

        if number_of_flawed_images != prev_number_of_flawed_images:
            # 3 new images incoming
            print(f'{number_of_flawed_images} and {prev_number_of_flawed_images}')
            time.sleep(0.8)
            analyze_one_car(car_counter)
            car_counter += 1
            number_of_flawed_images = prev_number_of_flawed_images + 3

        prev_number_of_flawed_images = number_of_flawed_images

except KeyboardInterrupt:
    pass
