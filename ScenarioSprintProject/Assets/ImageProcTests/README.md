# Prerequisites for running the Python image analyzer

First [set up a python virtual environment](https://docs.python.org/3/library/venv.html). Activate the virtual environment after you've created it.

Once your virtual environment is activated, run `pip install -r requirements.txt` in your terminal. This installs the script dependencies.

To test your environment run `python image-analyzer-test.py` in `Assets\ImageProcTests`

# Running the analyzer along side Unity

Run the scene `ImageProcTest` in the `Scenes` folder.

Go to `Window -> Paint in 3D -> Paint in Editor` and configure your brush. Set the Tool to the circular tool, Material to Normal_Bump and Shape to Circle D. Set the radius to approximate 0.1.

Run the scene and the enter the B key to get your ground truth image. Then paint bumps over the cube wherever you like, and hit the C key to capture the flawed image. 

Once you have both the images, run `python image-analyzer-test.py`. Unity should print out the SSIM on the console, and new images with contours should show up in the `Assets\ImageProcTests\Screenshots`