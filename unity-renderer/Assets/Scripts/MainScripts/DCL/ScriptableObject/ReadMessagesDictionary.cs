using UnityEngine;

/// <summary>
/// Stores last read messages from friends. Dictionaty[key='userId', value='timestamp of the last reading']
/// </summary>
[CreateAssetMenu(fileName = "ReadMessagesDictionary", menuName = "ReadMessagesDictionary")]
public class ReadMessagesDictionary : BaseDictionary<string, long>
{
}
