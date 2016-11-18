using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public Singleton main;
	public Map map; // the map/level which the player is in
	public Player otherPlayer; // the other player
	public AudioSource audioSource;
	public AudioClip deathSound;
	public AudioClip goalSound;
	public AudioClip stepSound;
	public AudioClip bumpSolidSound;
	public float stepPeriod; // number of seconds between player steps
	public bool isAtGoal;

	private const int maxScorePerLevel = 100;

	private int col;
	private int row;
	private float lastStepTime;
	private Vector3 animStartPos; // the position to start animating from
	private Vector3 animEndPos; // the position to animate towards

	public int StepsTakenCurrLevel {
		get;
		set;
	}

	// Use this for initialization
	void Start () {
		MoveToNextLevel();
		animStartPos = animEndPos;
	}

	// Update is called once per frame
	void Update() {

		float interpFrac = (Time.time - lastStepTime) / stepPeriod;
		transform.position = Vector3.Lerp (animStartPos, animEndPos, interpFrac);

		// is this player now at a goal tile?
		isAtGoal = map.IsCellGoal(col, row);

		if(Time.time - lastStepTime > stepPeriod)
		{
			animStartPos = animEndPos;

			// has this player stepped on a lethal tile?
			if(map.IsCellLethal(col, row))
			{
				// both players die
				audioSource.PlayOneShot(deathSound);
				KillPlayer();
				otherPlayer.KillPlayer();
				return;
			}
			
			// have both players reached a goal?
			if(isAtGoal && otherPlayer.isAtGoal)
			{
				// both players move to the next level
				audioSource.PlayOneShot(goalSound);
				MoveToNextLevel();
				otherPlayer.MoveToNextLevel();
				return;
			}

			// keyboard input
			float xInput = Input.GetAxis("Horizontal");
			float yInput = Input.GetAxis("Vertical");

			// touch input
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
			{
				Vector2 delta = Input.GetTouch(0).deltaPosition;
				xInput += delta.x / 5f;
				yInput += delta.y / 5f;
			}

			// pick direction of movement (and prevent diagonal movement!)
			int xMove = 0;
			int yMove = 0;
			if(Mathf.Abs(xInput) > Mathf.Abs(yInput))
				xMove = Mathf.RoundToInt(Mathf.Clamp(xInput, -1, +1));
			else
				yMove = Mathf.RoundToInt(Mathf.Clamp(yInput, -1, +1));

			// will players move?
			if(xMove != 0 || yMove != 0)
			{
				int newCol = col + xMove;
				int newRow = row - yMove; // yMove is +ve up instead of down
				if(map.IsCellSolid(newCol, newRow))
				{
					audioSource.PlayOneShot(bumpSolidSound);
				}
				else
				{
					// player moves a step
					audioSource.PlayOneShot(stepSound, 0.1f);
					col = newCol;
					row = newRow;
					animStartPos = transform.position;
					animEndPos = map.GetCellPos(col, row);
				}
				lastStepTime = Time.time;
				StepsTakenCurrLevel++;
			}
		}
	}

	public void MoveToNextLevel()
	{
		// TODO: do not penalise player for winning level!
		//		MyAdvertShower.ShowAdvert();

		map.MoveToNextLevel(maxScorePerLevel - StepsTakenCurrLevel);
		ResetPlayer();
	}

	private void KillPlayer() {
		main.FacebookInitAndPost();
//		MyAdvertShower.ShowAdvert();
		map.MoveToFirstLevel();
		ResetPlayer();
	}

	private void ResetPlayer() {
		var start = map.getPlayerStart();
		col = (int)start.x;
		row = (int)start.y;
		animStartPos = transform.position;
		animEndPos = map.GetCellPos(col, row);
		StepsTakenCurrLevel = 0;
	}
}
