using System.Collections;
using System.Windows.Forms;
using System;

// from: http://support.microsoft.com/kb/319401

/// <summary>
/// This class is an implementation of the 'IComparer' interface.
/// </summary>
public class ListViewColumnSorter : IComparer
{
  /// <summary>
  /// Specifies the column to be sorted
  /// </summary>
  private int ColumnToSort;
  /// <summary>
  /// Specifies the order in which to sort (i.e. 'Ascending').
  /// </summary>
  private SortOrder OrderOfSort;
  /// <summary>
  /// Case insensitive comparer object
  /// </summary>
  private CaseInsensitiveComparer ObjectCompare;

  /// <summary>
  /// Class constructor.  Initializes various elements
  /// </summary>
  public ListViewColumnSorter()
  {
    // Initialize the column to '0'
    ColumnToSort = 0;

    // Initialize the sort order to 'none'
    OrderOfSort = SortOrder.None;

    // Initialize the CaseInsensitiveComparer object
    ObjectCompare = new CaseInsensitiveComparer();
  }

  /// <summary>
  /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
  /// </summary>
  /// <param name="x">First object to be compared</param>
  /// <param name="y">Second object to be compared</param>
  /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
  public int Compare(object x, object y)
  {
    int compareResult;
    ListViewItem listviewX, listviewY;

    // Cast the objects to be compared to ListViewItem objects
    listviewX = (ListViewItem)x;
    listviewY = (ListViewItem)y;

    // Compare the two items
    if (listviewX.ListView.Name == "listViewEdits")
    {
      if ((ColumnToSort == 0) || (ColumnToSort == 4))
      {
        compareResult = ObjectCompare.Compare(Convert.ToInt32(listviewX.SubItems[ColumnToSort].Text), Convert.ToInt32(listviewY.SubItems[ColumnToSort].Text));
      }
      else
        compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
    }
    else if (listviewX.ListView.Name == "listViewUsers")
    {
      if ((ColumnToSort == 1) || (ColumnToSort == 2)) // nr of edits
      {
        compareResult = ObjectCompare.Compare(Convert.ToInt32(listviewX.SubItems[ColumnToSort].Text), Convert.ToInt32(listviewY.SubItems[ColumnToSort].Text));
      }
      else if ((ColumnToSort == 3) || (ColumnToSort == 6)) // percentage
      {
        string text1 = listviewX.SubItems[ColumnToSort].Text;
        string text2 = listviewY.SubItems[ColumnToSort].Text;
        if (text1 == "?") text1 = "0 %";
        if (text2 == "?") text2 = "0 %";
        compareResult = ObjectCompare.Compare(Convert.ToInt32(text1.Substring(0, text1.Length - 2)), Convert.ToInt32(text2.Substring(0, text2.Length - 2)));
      }
      else
        compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);
    }
    else
      compareResult = 0;

    // Calculate correct return value based on object comparison
    if (OrderOfSort == SortOrder.Ascending)
    {
      // Ascending sort is selected, return normal result of compare operation
      return compareResult;
    }
    else if (OrderOfSort == SortOrder.Descending)
    {
      // Descending sort is selected, return negative result of compare operation
      return (-compareResult);
    }
    else
    {
      // Return '0' to indicate they are equal
      return 0;
    }
  }

  /// <summary>
  /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
  /// </summary>
  public int SortColumn
  {
    set
    {
      ColumnToSort = value;
    }
    get
    {
      return ColumnToSort;
    }
  }

  /// <summary>
  /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
  /// </summary>
  public SortOrder Order
  {
    set
    {
      OrderOfSort = value;
    }
    get
    {
      return OrderOfSort;
    }
  }

}