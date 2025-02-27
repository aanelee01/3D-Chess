using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using Defs;

/// <summary>
/// Main storge for 3D array of cell objects.
/// </summary>
public class Board : MonoBehaviour
{
    /* ==========  MEMBERS  ========== */

    /// <summary> Prefab of the cell to be instantiated into the array. </summary>
    [SerializeField]
    private GameObject cell_prefab;
    /// <summary> xyz dimensions of the 3D array. </summary>
    public Vector3Int grid_dimensions = new Vector3Int(8, 8, 8);


    /// <summary> 3D array of cell objects. </summary>
    private Cell[,,] grid;

    private bool regen_moves = true;




    /* ==========  MAIN FUNCTIONS  ========== */

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // intialise array
        grid = new Cell[grid_dimensions.x, grid_dimensions.y, grid_dimensions.z];

        // iterate over every (x,y,z) coordinate triple. Instantiate a new cell for each coordinate. 
        for (int x = 0; x < grid_dimensions.x; x++)
        {
            // Cell positions will begin at -7 on each axis, range until 7, and have a step of 2.
            // this will make the board centred at (0,0,0)
            float xpos = (2f * x) - 7f;
            for (int y = 0; y < grid_dimensions.y; y++)
            {
                float ypos = (2f * y) - 7f;
                for (int z = 0; z < grid_dimensions.z; z++)
                {
                    // Instantiate cell prefab as a child of cell container, and access the Cell script
                    Cell cell = Instantiate(
                        cell_prefab,
                        new Vector3(xpos, ypos, (2f * z) - 7f),
                        Quaternion.identity,
                        transform.GetChild(0))
                            .GetComponent<Cell>();
                    // set the cell's indices
                    cell.index = new Vector3Int(x, y, z);

                    // commit hte finished cell to the array 
                    grid[cell.index.x, cell.index.y, cell.index.z] = cell;
                }
            }
        }

        // RegenerateMoves();
    }

    // Update is called once per frame
    void Update()
    {
        if (regen_moves)
        {
            RegenerateMoves();
            regen_moves = false;
        }
    }

    /// <summary>
    /// Get the cell at the given indices,
    /// </summary>
    /// <param name="index">xyz Vector3Int of the desired xyz indices</param>
    /// <returns>A reference to the cell at indices xyz. <c>null</c> if the indices are out of bounds.</returns>
    public Cell GetCellAt(Vector3Int index)
    {
        return GetCellAt(index.x, index.y, index.z);
    }

    /// <summary>
    /// Get the cell at the given indices.
    /// </summary>
    /// <param name="index">x, y, and z indices.</param>
    /// <returns>A reference to the cell at indices xyz. <c>null</c> if the indices are out of bounds.</returns>
    public Cell GetCellAt(int x, int y, int z)
    {
        // return null if index is out of bounds
        if (x < 0 || y < 0 || z < 0 || x >= grid_dimensions.x || y >= grid_dimensions.y || z >= grid_dimensions.z) return null;
        return grid[x, y, z];
    }




    /* ==========  HELPER FUNCTIONS  ========== */

    /// <summary>
    /// Toggles the index rendering for all cells in the grid.
    /// </summary>
    [ContextMenu("Toggle Indices")]
    public void ToggleIndices()
    {
        foreach (Cell cell in grid) cell.ToggleIndexDisplay();
    }

    [ContextMenu("Regenerate Moves")]
    public void RegenerateMoves()
    {
        foreach (Cell cell in grid)
        {
            cell.attackers = new Dictionary<TeamColour, List<Piece>>();
            // i know it says initialisation can be simplified, but trust me the alternative is ugly and less simple.
            cell.attackers.Add(TeamColour.White, new List<Piece>());
            cell.attackers.Add(TeamColour.Black, new List<Piece>());
        }
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");
        Stack<King> kings = new Stack<King>(); // kings will be handled after the other pieces, as this is necessary for how they avoid moving into check
        foreach (GameObject o in pieces)
        {
            Piece p = o.GetComponent<Piece>();
            if (p.Type == PieceType.King) kings.Push(p as King);
            p.RegenerateMoves();
        }
        while (kings.Count > 0) kings.Pop().RefineMoves();
    }

}