var FlashLight : Light;


function Update()

{

    if(Input.GetKeyDown(KeyCode.F))

    {

        FlashLight.enabled = !FlashLight.enabled;

    }

}