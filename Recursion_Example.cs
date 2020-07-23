	private class EnforcerInfo
	{
		public string desc;
		public bool isRoot, standardizeOffset;
		public HexCell cell;
		public int lowerAllowed, higherAllowed;
		public List<EnforcerInfo> enforcees;

		public EnforcerInfo(string desc, bool isRoot, bool standardizeOffset, HexCell cell, int lowerAllowed, int higherAllowed, EnforcerInfo enforcee)
		{
			this.desc = desc;
			this.isRoot = isRoot;
			this.standardizeOffset = standardizeOffset;
			this.cell = cell;
			this.lowerAllowed = lowerAllowed;
			this.higherAllowed = higherAllowed;
			enforcees = new List<EnforcerInfo>();
			enforcees.Add(enforcee);
		}

		public EnforcerInfo(string desc, bool isRoot, bool standardizeOffset, HexCell cell, int lowerAllowed, int higherAllowed)
		{
			this.desc = desc;
			this.isRoot = isRoot;
			this.standardizeOffset = standardizeOffset;
			this.cell = cell;
			this.lowerAllowed = lowerAllowed;
			this.higherAllowed = higherAllowed;
			enforcees = new List<EnforcerInfo>();
		}
	}

private List<FlattenInfo> flattenOptimal = new List<FlattenInfo>();

	private int GetFlattenCost(List<FlattenInfo> path, int costToLower, int costToRaise)
	{
		int cost = 0;
		for (int i = 0; i < path.Count; i++) cost += Mathf.Abs(path[i].elevationChange) * (path[i].elevationChange < 0 ? costToLower : costToRaise);
		return cost;
	}

	private List<FlattenInfo> GetOptimalFlattening(List<EnforcerInfo> enforcers, int minElevationChange, int maxElevationChange)
	{
		void ApplyTempElevationChange(List<EnforcerInfo> enforcees, int elevationChange)
		{
			for (int i = 0; i < enforcees.Count; i++)
			{
				enforcees[i].lowerAllowed += elevationChange;
				enforcees[i].higherAllowed += elevationChange;
			}
		}

		List<FlattenInfo> myOptimal = new List<FlattenInfo>(), myOptimalSub = new List<FlattenInfo>(), currentPathSub = new List<FlattenInfo>();
		for (int k = 0; k < enforcers.Count; k++)
		{
			myOptimalSub.Clear();
			currentPathSub.Clear();
			if (CheckCellValidForBuilding(enforcers[k].cell, false) && !enforcers[k].cell.Unit)
			{
				int elevationChange;
				//Debug.Log("Looping through " + enforcers[k].desc + " from " + (enforcers[k].cell.Elevation + minElevationChange).ToString() + " to " + (enforcers[k].cell.Elevation + maxElevationChange).ToString());
				//Debug.Log("Allowing " + enforcers[k].lowerAllowed.ToString() + " to " + enforcers[k].higherAllowed.ToString());
				for (int i = enforcers[k].cell.Elevation + minElevationChange; i <= enforcers[k].cell.Elevation + maxElevationChange; i++)
				{
					if (i >= enforcers[k].lowerAllowed && i <= enforcers[k].higherAllowed)
					{
						currentPathSub.Clear();
						elevationChange = i - enforcers[k].cell.Elevation;
						//Debug.Log("Checking " + enforcers[k].desc + " at level " + i.ToString());
						if (enforcers[k].enforcees.Count > 0)
						{
							ApplyTempElevationChange(enforcers[k].enforcees, elevationChange);
							currentPathSub.AddRange(GetOptimalFlattening(enforcers[k].enforcees, minElevationChange, maxElevationChange));
							ApplyTempElevationChange(enforcers[k].enforcees, -elevationChange);
						}
						currentPathSub.Add(new FlattenInfo(enforcers[k].desc, enforcers[k].cell, elevationChange, enforcers[k].standardizeOffset));
						//Debug.Log("Current cost: " + GetCost(currentPathSub).ToString() + " Optimal Cost: " + GetCost(myOptimalSub).ToString());
						if (myOptimalSub.Count == 0 || GetFlattenCost(currentPathSub, 1, 2) < GetFlattenCost(myOptimalSub, 1, 2))
						{
							//Debug.Log("New minimum: " + GetFlattenCost(currentPathSub).ToString() + " for " + enforcers[k].desc + " level " + i.ToString());
							myOptimalSub.Clear();
							myOptimalSub.AddRange(currentPathSub);
						}
					}
				}
			}
			if (myOptimalSub.Count == 0) myOptimalSub.Add(new FlattenInfo(enforcers[k].desc, enforcers[k].cell, 999, enforcers[k].standardizeOffset));
			myOptimal.AddRange(myOptimalSub);
		}
		return myOptimal;
	}

	bool TestBuilding(HexCell cell)
	{
		if (cell != null)
		{
			for (int i = 0; i < flattenOptimal.Count; i++) flattenOptimal[i].cell.DisableHighlight();
			enforcers.Clear();
			//cell.EnableHighlight(Color.green);
			EnforcerInfo mainEnforcer, tempEnforcer, tempEnforcer2;
			if (currentUIBuilding.ToFloorPlan() == FloorPlan.Diamond) // Builder
			{
				tempEnforcer = new EnforcerInfo("Opposite", false, true,
					cell.GetNeighbor(HexDirectionExtensions.Opposite(lastDirection)),
					cell.Elevation, cell.Elevation);
				mainEnforcer = new EnforcerInfo("Main", true, true, cell,
					cell.Elevation - 1, cell.Elevation + 1, tempEnforcer);
				tempEnforcer = new EnforcerInfo("Afdak", false, true,
					cell.GetNeighbor(HexDirectionExtensions.Previous2(lastDirection)),
					cell.Elevation, cell.Elevation);
				mainEnforcer.enforcees.Add(tempEnforcer);
				tempEnforcer = new EnforcerInfo("Open", false, false,
					cell.GetNeighbor(HexDirectionExtensions.Next2(lastDirection)),
					cell.Elevation - 1, cell.Elevation + 1);
				mainEnforcer.enforcees.Add(tempEnforcer);
				enforcers.Add(mainEnforcer);
			}

			flattenOptimal = GetOptimalFlattening(enforcers, -1, 1);
			int digs = GetFlattenCost(flattenOptimal, 1, 2);
			if (digs >= 999) allowed = false;
			RedYellowGreenHighlights(flattenOptimal);
		}
		return allowed;
	}