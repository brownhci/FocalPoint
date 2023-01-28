# FocalPoint: Adaptive Direct Manipulation for Selecting Small 3D Virtual Objects

FocalPoint is a direct manipulation technique in smartphone augmented reality (AR) for selecting small densely-packed objects within reach, a fundamental yet challenging task in AR due to the required accuracy and precision. FocalPoint adaptively and continuously updates a cylindrical geometry for selection disambiguation based on the user's selection history and hand movements. This design is informed by a preliminary study which revealed that participants preferred selecting objects appearing in particular regions of the screen. We evaluate FocalPoint against a baseline direct manipulation technique in a 12-participant study with two tasks: selecting a 3 mm wide target from a pile of cubes and virtually decorating a house with LEGO pieces. FocalPoint was three times as accurate for selecting the correct object and 5.5 seconds faster on average; participants using FocalPoint decorated their houses more and were more satisfied with the result. We further demonstrate the finer control enabled by FocalPoint in example applications of robot repair, 3D modeling, and neural network visualizations.

Read and cite our research paper at IMWUT 2023: https://doi.org/10.1145/3580856.

## Getting Started

### Step 1: Prerequisites

Follow the setup guide [here](https://github.com/brownhci/portalble) to install Unity and the base system for FocalPoint
 - Unity Version: 2020.1.2f1

### Step 2: Clone the repository to your computer
`git clone git@github.com:brownhci/FocalPoint.git`

### Step 3: Use Unity Hub to open the project folder

### Step 4: Open an example scene in Unity

You can find an example scene in `Assets/Jiaju/focus.unity`. The scenes used in our user study are `Assets/Jiaju/focus_task_1.unity` and `Assets/Jiaju/focus_task_12.unity`
