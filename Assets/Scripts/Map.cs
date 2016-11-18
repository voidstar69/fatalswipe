using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour {

	// public data members set via the Unity Editor

	public bool otherWorld; // left camera sees 'normal' world. right camera sees 'other' world.

	public Transform[] tiles; // prefabs used as tiles to build a map/level

	public bool[] tileIsSolid; // indicates if a tile is solid, i.e. player cannot move into this tile


	// rotation of the tiles in each world
	private readonly Quaternion normalTileRot = Quaternion.identity;
	private readonly Quaternion otherTileRot = Quaternion.Euler(0, 0, 180);

	// character representing the first tile
	private const char baseTileChar = 'a';

	// characters representing the special tiles
	private const char lethalTile = '+';
	private const char goalTile = '?';
	private const char startTile = '$';

	// maps a cell from levelBlueprint to an index into tiles[] and tileIsSolid[]
	private static readonly Dictionary<char, int> cellToTileMap = new Dictionary<char, int>
	{
		// tile behaviour:
		{ ' ', 0 }, // walkable for both players
		{ '.', 1 }, // walkable for both players
		{ '<', 2 }, // walkable for left player; solid for right player
		{ '>', 3 }, // solid for left player; walkable for right player
		{ '#', 4 }, // solid for both players
		{ lethalTile, 5 }, // kills both players
		{ goalTile, 6 }, // goal for both players
		{ startTile, 7 }, // starting point for both players
		// NOTE: default player start point (if no 'start tile' is present) is in column 3, row 3
	};

	private static readonly string[][] levelBlueprint =
	{
/*
		// new level ? - a split maze. TODO: players cannot both touch different goals simultaneously. Players end up at least 3 tiles apart!
		new[] {
			"     #        .#",
			"## # # ###### ",
			"   # # # ?#?  ...",
			"$##  # # #####",
			"#   #  #      ..",
			"# ## >#######<",
			"              .",
		},
*/

		// level A - mostly empty level with few solid tiles, and an goal that is easy to get to
		new[] {
			"  . . # ",
			"#  # . .",
			"  . .#. ",
			"...#. ? ",
			"$ ## . .",
			". . .# .",
		},

		// level B - synced path to goal, only 1 lethal tile (easily avoidable)
		new[] {
			"#  .. #",
			". #+#  ",
			".#$#?#.",
			".  # ..",
		},

		// level C - synced path to goal, but with lots of lethal tiles
		new[] {
			"   ++   ",
			" # #? + ",
			" #$#+++ ",
			"  #   # ",
			"+   +   ",
		},

		// level D - separate paths for each player, easy with no lethal tiles
		new[] {
			" . . # ",
			".#>#.#.",
			"## # . ",
			". <#.#.",
			" . . #?",
		},

		// level E - tricky navigation of each player along a different path
		new[] {
			" . < + ",
			".++#  ?",
			"<. #...",
			"<>#>.+.",
			"<>.>.+.",
		},

		// level F - out-of-sync movement to goal, with lots of lethal tiles
		new[] {
			"?    ++",
			"++++  +",
			" < >+  ",
			"+.+.+..",
			"+...+><",
			"+>+....",
		},

		// level D2 - now boundary is lethal. Medium difficulty.
		// TODO: mirror level left-to-right
		new[] {
			"+++++++++",
			"+ . . #?+",
			"+. <#. .+",
			"+##$# . +",
			"+.#>#.#.+",
			"+ . . # +",
			"+++++++++",
		},

		// level F2 - now boundary is lethal. Fairly difficult.
		new[] {
			"++++<++++",
			"..?   +++",
			"+++++  ++",
			"+ <$>+  +",
			"++.+.+..+",
			"++...+><+",
			"++>+....+",
			"+++++++++",
		},

		// level E2 - now boundary is lethal. Very difficult.
		new[] {
			"+++++++++",
			"+ . < + +",
			"+.++#  ?#",
			" <.$#...+",
			"+<># .+.+",
			"+<>.>.+.+",
			"+++++++++",
		},

		// level G - the final level, so no goal (but a lethal tile to reset game)
		new[] {
			"   .$   . . . .    ..   . . . ",
			".<<<.>.>.<<<. >>> <<. < >>.  . ",
			"..<..>>> <>>  ><< <.< < > > . .+",
			"  <..>.> <<< .>>>.<..<< >>.   .",
			"   ..   . . . .    ..   . . . ",
		},
	};

	private static readonly int numLevels = levelBlueprint.GetLength(0);

	// TODO: jump to a level, for debugging
	//private int currLevelIndex = 7;

	private int currLevelIndex = -1; // current level's index into levelBlueprint array, to produce levelGrid
	private string[] levelGrid; // jagged grid of current level, one char per grid tile
	private List<Transform> levelTiles = new List<Transform>(); // current level tiles materialised as visible objects

	public int Score {
		get;
		set;
	}

	// Use this for initialization
	void Start()
	{
		// Player moves us to first level, before it calls getPlayerStart

		Score = 0;
	}

	public Vector2 getPlayerStart()
	{
		// locate the player start tile
		int row = 0;
		foreach(var rowOfCells in levelGrid)
		{
			int col = 0;
			foreach(var cell in rowOfCells)
			{
				if(startTile == cell)
					return new Vector2(col, row);
				col++;
			}
			row++;
		}

		// if player start is missing, use a default start position
		return new Vector2(2, 2);
	}

	public void MoveToFirstLevel()
	{
		// move to the first level
		currLevelIndex = -1;
		MoveToNextLevel(0);
		Score = 0;
	}

	public void MoveToNextLevel(int scoreIncrease)
	{
		Score += scoreIncrease;
		DestroyLevel();
		currLevelIndex = (currLevelIndex + 1) % numLevels;
		levelGrid = levelBlueprint[currLevelIndex];
		BuildLevel();
	}

	public bool IsCellOutOfBounds(int col, int row)
	{
		// is cell outside level boundary?
		return (col < 0 || row < 0 || row >= levelGrid.GetLength(0) || col >= levelGrid[row].Length);
	}

	public bool IsCellSolid(int col, int row)
	{
		// cells outside level boundary are solid
		if(IsCellOutOfBounds(col, row))
			return true;

		// test whether this (internal) cell is solid or empty
		char cell = levelGrid[row][col];
		int cellTileIndex = cellToTileMap[cell];
//		int cellTileIndex = cell - baseTileChar;
		return tileIsSolid[cellTileIndex];
	}

	public bool IsCellLethal(int col, int row)
	{
		// cells outside level boundary are not lethal
		if(IsCellOutOfBounds(col, row))
			return false;
		
		// test whether this (internal) cell is lethal or not
		return lethalTile == levelGrid[row][col];
	}

	public bool IsCellGoal(int col, int row)
	{
		// cells outside level boundary are not the goal
		if(IsCellOutOfBounds(col, row))
			return false;
		
		// test whether this (internal) cell is the goal or not
		return goalTile == levelGrid[row][col];
	}

	// get the position of this cell in the world
	public Vector3 GetCellPos(int col, int row)
	{
		// offset the 'other' world by a number of rows
		// Unity y+ is up instead of down
		return new Vector3(col, -row - (otherWorld ? 100 : 0), 0);
	}

	private void BuildLevel()
	{
		int row = 0;
		foreach(var rowOfCells in levelGrid)
		{
			int col = 0;
			foreach(var cell in rowOfCells)
			{
				int cellTileIndex = cellToTileMap[cell];
//				int cellTileIndex = cell - baseTileChar;
//				Debug.Log(string.Format("{0},{1}: {2},{3}", x, y, cell, cellTileIndex));
				var tile = tiles[cellTileIndex];
				var newTransform = Instantiate(tile, GetCellPos(col, row), (otherWorld ? otherTileRot : normalTileRot));
				levelTiles.Add(newTransform as Transform);
				col++;
			}
			row++;
		}
	}

	private void DestroyLevel()
	{
		foreach(var transform in levelTiles)
		{
			Object.Destroy(transform.gameObject);
		}
		levelTiles.Clear();
	}
}