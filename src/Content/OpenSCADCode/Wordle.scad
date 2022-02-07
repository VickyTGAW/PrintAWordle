
guesses=4;
cubeSize=3;
margin=0.75;
boardThickness=1;
includeBacking = true;
includeUnusedGuesses = true;

//This is an array of all the letter guesses.  The numbers
//are the height the letter will be printed at (which facilitates
//the color changes). The actual heights are configurable. The order
//is going to be consistent:
//Lowest Height - Wrong guesses (grey/black)
//Middle Height - Misplaced Letters (yellow)
//Highest Height - Correct Letters (green)
letters = [[0.5,0.5,0.5,0.5,0.5],[1,1,0.5,0.5,0.5],[1.5,1,1,1,1.5],[1.5,1.5,1.5,1.5,1.5]];

//The width of our board is going to be five cubes (for each of the letters)
//And six margins (the space around the letters)
boardWidth = (cubeSize * 5) + (margin*6);

//The board size depends if we are going to include unused guesses/words.abs
//If are including that, it'll be six cube lengths (for the six guesses)
//and seven margins.  Otherwise, we'll go by the number of guesses.  There 
//will always be one extra margin than cubes.
boardHeight=  includeUnusedGuesses ? (cubeSize * 6) + (margin*7) : (cubeSize*guesses) + (margin * (guesses+1));

echo(boardWidth);
echo(boardHeight);

//We make a cube-- this is our backing for all our letters/words.
cube([boardWidth, boardHeight, boardThickness]);

//Now we are going to loop through all our guesses and each letter 
//for those guesses to make our letter cubes. Each cube will be positioned
//(using translate).  The height is determined by our letter array.
for (i = [0:guesses-1]) 
    { 
        for (j = [0:4])
        {
            //Retrieving the cube thickness from the array
            cubeThickness = letters[i][j];


            //Making the cube and putting it in the right place.
            //The 4-j makes sure the letters go from right to left 
            translate([((4-j)*cubeSize)+(((4-j)-1)*margin)+margin*2,(i*cubeSize)+((i-1)*margin)+margin*2, boardThickness])
            cube([cubeSize, cubeSize, cubeThickness]);
        }
        
        
    }

//For Earrings and Pendants, we are also going to include a small
//backing with a hole
if (includeBacking)
{
    //We will be subtracting a 2mm diameter cylinder from our 
    //backing
    difference()
    {

        translate([0, -3, 0])
        cube([boardWidth, 3, boardThickness]);
        
        $fn=32;
        translate([boardWidth/2,-1,0])
        cylinder(4, 1, true);
    }
}