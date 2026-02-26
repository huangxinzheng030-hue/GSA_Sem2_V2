//**********************Palace Interior-Salon Carré Pack **********************//

Created By Chung the Artist 2021 All rights reserved

Unity version:2019.4.5f1



---------------------------HOW TO USE---------------------------

Follow the step to revert the sample scene to the original looks if problem occurs

	FOR UNIVERSAL RENDER PIPELINE

		1. Put all the elements inside "For URP" folder into your project folder
		
		2. Install dependency "Substance in Unity" in your project (Optional), if you want to play with Substance material
		
		3. Go to Edit/Project Settings/Player/Other Settings change Color Space to Linear(Optional)

		4. Go to Window/Package Manager and install "Post Processing" asset into your project
		
		5. Make sure all the objects in the scene which inside "Static Mesh" are static
		
		6. Bake All the Reflection Probe
		
		7. Restart Unity application
		
		8. Enjoy 
		
	FOR HIGH DEFINITION RENDER PIPELINE
	
		1. Put all the elements inside "For HDRP" folder into your project folder
		
		2. Install dependency "Substance in Unity" in your project (Optional), if you want to play with Substance material
		
		3. Go to Edit/Project Settings/Graphics and use the file in 
		
		For HDRP/Settings/"HDRenderPipelineAsset" to replace the default Scriptable Render Pipeline Setting file
		
		4. Find Post Process Volume object, and replace the Post Process Volume Profile with "SampleScenePostProcessingSettings" which in the same directory as the previous item
		
		5. Find Sky and Fog Volume, replace Volume profile "Sky and Fog Settings" which all in the same directory as the previous item
		
		6. Make sure all the objects in the scene which inside "Static Mesh" are static
		
		7. Bake All the Reflection Probe
		
		8. Restart Unity application
		
		9. Enjoy


