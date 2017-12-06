using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUIManager : CModule {

	private static CUIManager instance;

	private Stack<int> openedStack;

	private Dictionary<int, CUIBase> dicUI;
	
	//根据使用频率来对UI进行排序，不常使用的UI就优先回收
	private Dictionary<int, int> uiFrequency;

	private int gcTimer = 100;

	private int gcLimit = 5;

	private List<int> closedList;

	public static CUIManager Instance{
		get{
			return instance;
		}
	}


	public override void Init()
	{
		instance = new CUIManager();

		dicUI = new Dictionary<int, CUIBase>();
		openedStack = new Stack<int>();
		uiFrequency = new Dictionary<int, int>();
		closedList = new List<int>();
	}

	public override void Update()
	{
		gcTimer--;
		if(gcTimer == 0)
		{
			gcTimer = 100;
			int len = closedList.Count;
			if(len > gcLimit)
			{
				for(int i = len - 1; i > gcLimit - 1; i--)
				{
					dicUI[closedList[i]].Dispose();
				}
				closedList.RemoveRange(gcLimit - 1, len - gcLimit);
			}else{
				dicUI[closedList[len - 1]].Dispose();
				closedList.RemoveAt(len-1);
			}
		}
	}

	public void OpenUI(int uiKey, params object[] args)
	{
		CUIBase ui;
		if(!dicUI.TryGetValue(uiKey, out ui))
		{
			ui = new CUIBase(uiKey);

			dicUI.Add(uiKey, ui);
		}

		int freq = 0;
		if(!uiFrequency.TryGetValue(uiKey, out freq))
		{
			uiFrequency.Add(uiKey, 1);
		}else{
			uiFrequency[uiKey] = freq + 1;
		}

		openedStack.Push(uiKey);

		switch(ui.State)
		{
			case CUIState.None:
				ui.Init();
				ui.Show();
				break;
			case CUIState.Inited:
				ui.Show();
				break;
			case CUIState.Show:
				ui.Refresh();
				break;
		}

	}

	public void CloseUI(int uiKey)
	{
		CUIBase ui;
		if(dicUI.TryGetValue(uiKey, out ui))
		{
			ui.Close();

			if(closedList.Contains(uiKey))
			{
				closedList.Add(uiKey);
				//每次关闭UI的时候，对列表进行排序
				closedList.Sort((x1,x2)=>{return uiFrequency[x2] - uiFrequency[x1];});
			} 
		}

		//如果关闭的是最后打开的UI，那么就把这个删掉。
		if(openedStack.Peek() == uiKey)
		{
			openedStack.Pop();
		}
	}

}
