void Main_float(float2 playerPosition , float2 boxCornerA , float2 boxCornerB , float2 boxCornerC , float2 boxCornerD , float2 boxCenter, out float Out)
{



}

void WT()
{

// check weird cases
// x

if (playerPosition.x > 10 || playerPosition.y > -10)
    {
        playerPosition.x = playerPosition.x / 10;
    }

if (playerPosition.x > 10)
	{
		playerPosition.x = playerPosition.x / 10;
	}


if (playerPosition.x > -10)
	{
		playerPosition.x = playerPosition.x / 10;
	}

// y
if (playerPosition.y > 10)
    {
        playerPosition.y = playerPosition.y / 10;
    }
if (playerPosition.y > -10)
    {
        playerPosition.y = playerPosition.y / 10;
    }


// check base case
if (playerPosition.x == 0 && playerPosition.y == 0)

    {
        Out = false;
        
    }


// check if player x goes out of 128
if (playerPosition.x > boxCornerA.x  )

    {

	Out = true;
  

    }


}

void WorksinCSharp()
{
    if (playerPosition.x > boxCornerA.x && playerPosition.y > boxCornerA.y)
        {
            
            Out = true;
        }
        else if (playerPosition.x > boxCornerB.x && playerPosition.y < boxCornerB.y)
        {
            
            Out = true;
        }
        else if (playerPosition.x < boxCornerC.x && playerPosition.y < boxCornerC.y)
        {
            
            Out = true;
        }
        else if (playerPosition.x < boxCornerD.x && playerPosition.y > boxCornerD.y)
        {
           
            Out = true;
        }
        else
        {
            
            Out = false;
        }
}

void HLSLWorking()
{

// checking zero zero position
if(playerPosition.x == 0 && playerPosition.y == 0)
{
Out = false;
}

// checking weird cases for x
if(playerPosition.x > 10)
{
playerPosition.x = playerPosition.x / 10;
}

if(playerPosition.x < -10)
{
playerPosition.x = playerPosition.x / 10;
}

// checking weird cases for y
if(playerPosition.y > 10)
{
playerPosition.y = playerPosition.y / 10;
}

if(playerPosition.y < -10)
{
playerPosition.y = playerPosition.y / 10;
}

// checking x to 128
if(playerPosition.x > boxCornerA.x || playerPosition.y > boxCornerA.y)
{
Out = true;
}

// checking x to -128
if(playerPosition.x <  boxCornerC.x || playerPosition.y < boxCornerC.y)
{
Out = true;
}

}



        
