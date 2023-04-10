using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace redd096
{
	public class WindowParseCSVData : ScriptableObject
	{
		const string DATANAME = "Window Parse CSV Data.asset";

		public int IndexStruct = 0;
		public ParseCSV ParseClass = new ParseCSV();

		#region static functions

		/// <summary>
		/// Get Path to this scriptable object saved in project folder
		/// </summary>
		/// <returns></returns>
		public static string GetDataPath()
		{
			//find script folder and create scriptable object there
			string[] guids = AssetDatabase.FindAssets(typeof(WindowParseCSVData).ToString());
			foreach (string guid in guids)
			{
				string pathToScript = AssetDatabase.GUIDToAssetPath(guid);                                                                          //get path to the script
				string pathToFolder = pathToScript.Remove(pathToScript.LastIndexOf('/'), pathToScript.Length - pathToScript.LastIndexOf('/'));      //remove everything from last slash (script name) to get only directory path

				return Path.Combine(pathToFolder, DATANAME);
			}

			return null;
		}

		/// <summary>
		/// Load or Create Scriptable Object
		/// </summary>
		/// <returns></returns>
		public static WindowParseCSVData LoadData()
		{
			//try get scriptable object (data)
			WindowParseCSVData data = AssetDatabase.LoadAssetAtPath<WindowParseCSVData>(GetDataPath());

			//if there is no data, create it
			if (data == null)
			{
				data = CreateInstance<WindowParseCSVData>();
				AssetDatabase.CreateAsset(data, GetDataPath());
			}

			//load data
			return data;
		}

		#endregion
	}

	[Serializable]
	public class ParseCSV
	{
		//every row is a scriptable object
		//every column is a variable (or array)

		[Header("CSV")]
		[TextArea(2, 5)]
		public string DefaultCSV = "";                                              //downloaded CSV
		[SerializeField] KeyValueStruct[] privateParsedCSV = default;               //at [row][column] there is excel cell item - can be a variable (item) or array (item1;item2;item3;)
		[SerializeField] KeyKeyValueStruct[] privateArrayParsedCSV = default;       //every [row][column] is an array - a variable ([0] = item) or array ([0] = item1, [1] = item2, [2] = item3)

		[Header("First Parse")]
		public bool TrimParsedElements = true;

		[Header("Split Arrays")]
		public bool SplitArrays = true;
		public string StringToSplitArrayElements = ";";
		public bool TrimArrayElements = true;
		public bool RemoveEmptyArrayElements = true;
		public bool RemoveDuplicates = true;

		[Header("Dictionary")]
		public bool AllCapsDictionaryValues = true;
		public bool NoSpacesDictionaryValues = true;
		[SerializeField] string[] privateColumnsDictionary = default;

		public string[][] ParsedCSV
		{
			get
			{
				//if privateParsedCSV is null, return null
				if (privateParsedCSV == null)
					return null;

				//else return privateParsedCSV
				string[][] temp = new string[privateParsedCSV.Length][];
				for (int i = 0; i < privateParsedCSV.Length; i++)
					temp[i] = privateParsedCSV[i].Value;

				return temp;
			}
			set
			{
				//if value is null, set null
				if (value == null)
				{
					privateParsedCSV = null;
					return;
				}

				//else set privateParsedCSV
				privateParsedCSV = new KeyValueStruct[value.Length];
				for (int i = 0; i < value.Length; i++)
					privateParsedCSV[i].Value = value[i];
			}
		}
		public string[][][] ArraysParsedCSV
		{
			get
			{
				//if privateArrayParsedCSV is null, return null
				if (privateArrayParsedCSV == null)
					return null;

				//else return privateArrayParsedCSV
				string[][][] temp = new string[privateArrayParsedCSV.Length][][];
				for (int i = 0; i < privateArrayParsedCSV.Length; i++)
				{
					//if privateArrayParsedCSV[i].Values is null, this element is null
					if (privateArrayParsedCSV[i].Values == null)
					{
						temp[i] = null;
						continue;
					}

					//else get element from privateArrayParsedCSV[i].Values
					temp[i] = new string[privateArrayParsedCSV[i].Values.Length][];
					for (int j = 0; j < privateArrayParsedCSV[i].Values.Length; j++)
					{
						temp[i][j] = privateArrayParsedCSV[i].Values[j].Value;
					}
				}

				return temp;
			}
			set
			{
				//if value is null, set null
				if (value == null)
				{
					privateArrayParsedCSV = null;
					return;
				}

				//else set privateArrayParsedCSV
				privateArrayParsedCSV = new KeyKeyValueStruct[value.Length];
				for (int i = 0; i < value.Length; i++)
				{
					//if value[i] is null, this element is null
					if (value[i] == null)
					{
						privateArrayParsedCSV[i].Values = null;
						continue;
					}

					//else get element from value[i]
					privateArrayParsedCSV[i].Values = new KeyValueStruct[value[i].Length];
					for (int j = 0; j < value[i].Length; j++)
					{
						privateArrayParsedCSV[i].Values[j].Value = value[i][j];
					}
				}
			}
		}

		#region public API

		/// <summary>
		/// Create Dictionary, ParsedCSV and ArraysParsedCSV
		/// </summary>
		public void Parse()
		{
			//from csv to rows, then [rows][columns]
			string[] rows = SplitInRows(DefaultCSV);
			ParsedCSV = SplitInColumns(rows);

			//create a dictionary, so can reorder columns on excel without problem
			privateColumnsDictionary = CreateDictionary(ParsedCSV);

			//if split arrays, split every string in array
			if (SplitArrays)
			{
				ArraysParsedCSV = SplitArray(ParsedCSV);
			}
		}

		/// <summary>
		/// Get value from Dictionary
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public int ColumnsDictionary(string key)
		{
			//if key inside dictionary, return value
			for (int i = 0; i < privateColumnsDictionary.Length; i++)
			{
				if (privateColumnsDictionary[i].Equals(key))
				{
					return i;
				}
			}

			return -1;
		}

		#endregion

		#region private API

		/// <summary>
		/// Split csv in rows (every '\n')
		/// </summary>
		string[] SplitInRows(string csv)
		{
			//character for new line ( normally \n )
			return csv.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
		}

		/// <summary>
		/// Split a row in columns (every ',')
		/// </summary>
		/// <param name="row"></param>
		/// <returns></returns>
		string[] SplitInColumns(string row)
		{
			//split row in columns
			string[] columns = row.Split(',');

			return ModifiedElement(columns);
		}

		/// <summary>
		/// Split every row in columns (every ',')
		/// </summary>
		/// <param name="rows"></param>
		/// <returns></returns>
		string[][] SplitInColumns(string[] rows)
		{
			string[][] rowsColumns = new string[rows.Length][];

			//split every row
			for (int i = 0; i < rows.Length; i++)
			{
				rowsColumns[i] = SplitInColumns(rows[i]);
			}

			return rowsColumns;
		}

		/// <summary>
		/// Create a dictionary using index (int) as Key, and string as Value
		/// </summary>
		/// <param name="firstValues"></param>
		/// <returns></returns>
		string[] CreateDictionary(string[] firstRow)
		{
			string[] dictionary = new string[firstRow.Length];

			//use index as Key, and string as Value
			for (int i = 0; i < firstRow.Length; i++)
			{
				dictionary[i] = ModifiedDictionaryValue(firstRow[i]);
			}

			return dictionary;
		}

		/// <summary>
		/// Create a dictionary from first row using index (int) as Key, and string as Value
		/// </summary>
		/// <param name="splittedRows"></param>
		/// <returns></returns>
		string[] CreateDictionary(string[][] rowsColumns)
		{
			//create dictionary from first row
			return CreateDictionary(rowsColumns[0]);
		}

		/// <summary>
		/// Split string in array
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		string[] SplitArray(string array)
		{
			//split array in a list
			string[] splittedArray = array.Split(new string[] { StringToSplitArrayElements }, StringSplitOptions.None);

			return ModifiedArray(splittedArray);
		}

		/// <summary>
		/// Split all parsed csv in arrays
		/// </summary>
		/// <param name="rowsColumns"></param>
		/// <returns></returns>
		string[][][] SplitArray(string[][] rowsColumns)
		{
			string[][][] parsedArrays = new string[rowsColumns.Length][][];

			for (int row = 0; row < rowsColumns.Length; row++)
			{
				parsedArrays[row] = new string[rowsColumns[row].Length][];

				for (int column = 0; column < rowsColumns[row].Length; column++)
				{
					//set every cell at [row][column] as an array
					parsedArrays[row][column] = SplitArray(rowsColumns[row][column]);
				}
			}

			return parsedArrays;
		}

		#region modify API

		string[] ModifiedElement(string[] splittedRow)
		{
			if (TrimParsedElements)
			{
				//if necessary - foreach column, split it
				for (int i = 0; i < splittedRow.Length; i++)
					splittedRow[i] = splittedRow[i].Trim();
			}

			return splittedRow;
		}

		string ModifiedDictionaryValue(string dictionaryValue)
		{
			//all caps
			if (AllCapsDictionaryValues)
				dictionaryValue = dictionaryValue.ToUpper();

			//remove white spaces
			if (NoSpacesDictionaryValues)
				dictionaryValue = dictionaryValue.Replace(" ", string.Empty);

			return dictionaryValue;
		}

		string[] ModifiedArray(string[] splittedArray)
		{
			//put array in a list
			List<string> splittedList = new List<string>(splittedArray);

			if (TrimArrayElements)
			{
				//if necessary - foreach array, split it
				for (int i = 0; i < splittedList.Count; i++)
					splittedList[i] = splittedList[i].Trim();
			}

			if (RemoveEmptyArrayElements)
			{
				//if necessary - foreach array, if empty remove from list
				foreach (string s in new List<string>(splittedList))
					if (string.IsNullOrWhiteSpace(s))
						splittedList.Remove(s);
			}

			if (RemoveDuplicates)
			{
				//if necessary - remove duplicates (create hashSet that remove duplicates by default and recreate a list from it)
				HashSet<string> listNoDuplicates = new HashSet<string>(splittedList);
				splittedList = new List<string>(listNoDuplicates);
			}

			return splittedList.ToArray();
		}

		#endregion

		#endregion
	}

	[Serializable]
	public struct KeyValueStruct
	{
		public string[] Value;
	}

	[Serializable]
	public struct KeyKeyValueStruct
	{
		public KeyValueStruct[] Values;
	}
}