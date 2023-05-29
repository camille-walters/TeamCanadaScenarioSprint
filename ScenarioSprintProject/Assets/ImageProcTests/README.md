# Prerequisites for running the Python image analyzer

First [set up a python virtual environment](https://docs.python.org/3/library/venv.html). Activate the virtual environment after you've created it.

Once your virtual environment is activated, run `pip install -r requirements.txt` in your terminal. This installs the script dependencies.

To test your environment run `python image-analyzer-test.py` in `Assets\ImageProcTests`

# Running the analyzer along side Unity

Open the scene `ImageProcTest` in the `Scenes` folder. Set any camera positions under the CameraPositions gameobject. Currently it takes a left, right and top view shot of the cylinder.

Go to `Window -> Paint in 3D -> Paint in Editor` and configure your brush. Set the Tool to the circular tool, Material to Normal_Bump and Shape to Circle D. Set the radius to approximate 0.1. (Note: The brush sometimes loses its settings on play, you might have to reset them)

Run the scene and the enter the B key to get your ground truth images from all views. Then paint bumps over the cube wherever you like, and hit the C key to capture the corresponding flawed images. 

Once you have both the images, run `python image-analyzer-test.py`. Unity should print out the average SSIM of all views on the console, and new images with contours, difference and threshold should show up in the `Assets\ImageProcTests\Images`