using UnityEngine;

namespace FullRig
{
	public class BuildTelegraph : MonoBehaviour
	{
		public Transform[]	connections;
		public Rope			rope;

		Transform GetConnection(Transform tm, string name)
		{
			for ( int i = 0; i < tm.childCount; i++ )
			{
				if ( tm.GetChild(i).name == name )
					return tm.GetChild(i);
			}

			return null;
		}

		void Start()
		{
			for ( int i = 0; i < transform.childCount - 1; i++ )
			{
				Transform pole = transform.GetChild(i);
				Transform pole1 = transform.GetChild(i + 1);

				for ( int j = 0; j < connections.Length; j++ )
				{
					Transform c = GetConnection(pole, connections[j].name);
					Transform c1 = GetConnection(pole1, connections[j].name);

					if ( c && c1 )
					{
						GameObject robj = Instantiate(rope.gameObject);
						robj.SetActive(true);
						robj.transform.parent = c;
						Rope r = robj.GetComponent<Rope>();

						r.SetStartAttach(c);
						r.SetEndAttach(c1);
					}
				}
			}
		}
	}
}