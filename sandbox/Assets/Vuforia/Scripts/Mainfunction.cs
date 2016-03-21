using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class Mainfunction : MonoBehaviour {
	public GameObject frame;
	private int num = 0;
	private Rigidbody rigidbody;
	public static Hashtable db = new Hashtable(); //DataBase

	// Use this for initialization
	void Start () {
		//フレームマーカ変数登録
		db.Add("FrameMarker0","lost");
		db.Add("FrameMarker1","lost");
		db.Add("FrameMarker2","lost");
		db.Add("FrameMarker3","lost");
		db.Add("FrameMarker4","lost");
		db.Add("FrameMarker5","lost");
	}
	
	// Update is called once per frame
	void Update () {
		/* ########## 主に使う関数など ##########
		if(db["FrameMarker2"] == "found"){
			frame = GameObject.Find("framemarker (1)"); //framemarker1の検索
		    GameObject child = Instantiate(mycube,transform.position,transform.rotation) as GameObject; //cubeを作成
			child.name = "cube" + num.ToString(); //cubeの名前はcube<num>
			child.transform.parent = frame.transform; //cube<num>をframeの子要素に
			GameObject cube1 = GameObject.Find("cube1"); //cube1を取得
			if(cube1 != null){ //cube1が存在している場合
				rigidbody = cube1.GetComponent<Rigidbody>(); //cube1のrigidbodyにアクセス。rigidbody:性質
				rigidbody.drag = 1; //drag…空気抵抗値。デフォルトはinfinity(無限大)
			}
			num++;
		}
		########## ########## */

		//ここから主処理
		using(StreamReader r = new StreamReader(@"Assets/Sketch.txt")){ //Sketch.txtの読み取り準備
			//Sketch.txtを1行ずつ読み取り、その行に対応した処理をする。コンパイラ的な
			string line;
			int ifcount = 0; //ifカウンタ：if文でfalseのときに、中の処理を飛ばすときに使う

			while( (line = r.ReadLine()) != null){
				string[] csvdata = line.Split(','); //行を,で分割
				if(ifcount == 0){ //ifがfalseの時は中を飛ばす=ifカウンタ0以外の時は処理を実行しない
					switch(csvdata[0]){ //列1は関数・代入・制御文…の操作識別。いずれ消すかも
					case "Func": //関数
						switch(csvdata[1]){ //関数の列2は関数名
						case "add": //オブジェクト配置関数
							frame = GameObject.Find(getvalue(csvdata[2])); //add関数の列3はフレームマーカの種類
							GameObject prefab = (GameObject)Resources.Load ("Prefabs/" + getvalue(csvdata[3])); //add関数の列4はオブジェクトの種類
						    GameObject child = Instantiate(prefab,transform.position,transform.rotation) as GameObject; //配置
							child.transform.parent = frame.transform; //子要素化
							break;
						case "print": //標準出力(デバッグ用)
							print(getvalue(csvdata[2])); //print関数の列3は出力内容
							break;
						}
						break;
					case "Cont": //制御文
						switch(csvdata[1]){ //制御文の列2は制御の種類
						case "if": //if文
							switch(csvdata[2]){ //if文の列3は演算子(=,<,>,<=,>=)。未完
							case "eq": //=
								if(getvalue(csvdata[3]) != getvalue(csvdata[4])){ //イコールでないなら
									ifcount++; //ifカウンタ+1
								}
								break;
							}
							break;
						}
						break;
					case "Subs": //代入
						switch(csvdata[1]){ //代入の列2は変数の型(数値か文字列)
						case "num": //数値代入
							if(csvdata[2] == null){ //DBにその変数名が登録されていないならば
								db.Add(csvdata[2],simsolve(csvdata[3])); //列3(変数名)で列4(値)をDBに登録
							}else{
								db[csvdata[2]] = simsolve(csvdata[3]); //DB値の変更(値の上書き)
							}
							break;
						case "str": //文字列代入
							if(csvdata[2] == null){
								db.Add(csvdata[2],csvdata[3]);
							}else{
								db[csvdata[2]] = strsolve(csvdata[3]);
							}
							break;
						}
						break;
					}
				}else{ //ifがfalseにつきスキップ中。if文の中にさらにif文という入れ子構造を考慮
					if(csvdata[1] == "if"){ //スキップ中にさらにif文があればカウンタ+1
						ifcount++;
					}else if(csvdata[1] == "endif"){ //endif(if文の終わり)を通過する度にカウンタ-1
						ifcount--;
					}
				}
			}
		}
	}

	string getvalue(string variable){
		//" "…文字列,@…マーカーの状態(found/lost),#…マーカー,数値,変数　の解釈
		int len = variable.Length;
		double d;
		if(variable.Substring(0,1)=="\"" && variable.Substring(len-1,1)=="\""){
			return variable.Substring(1,len-2);
		}else if(variable.Substring(0,1)=="@"){
			return (string)db["FrameMarker" + getvalue(variable.Substring(1))];
		}else if(variable.Substring(0,1)=="#"){
			return "framemarker (" + getvalue(variable.Substring(1)) + ")";
		}else if(double.TryParse(variable,out d)){
			return variable;
		}else{
			return db[variable].ToString();
		}
	}

	double simsolve(string formula){
		//()なしの四則演算関数

		String split;
		int i = 0;

		//割り算がなくなったら=1つの数字になったら終了
		while(formula.IndexOf("/") != -1){
			//演算子区切りで分割
			split = formula.Replace("+",",+,");
			split = split.Replace("~",",~,");
			split = split.Replace("*",",*,");
			split = split.Replace("/",",/,");
			//区切りを配列化
			string[] formula_data = split.Split(',');

			if(formula_data[i] == "/"){
				formula_data[i] = (double.Parse(getvalue(formula_data[i-1])) / double.Parse(getvalue(formula_data[i+1]))).ToString();
				formula_data[i-1] = null;
				formula_data[i+1] = null;
				formula = bond(formula_data);
				i = 0;
			}
			i++;
		}

		//掛け算がなくなったら=1つの数字になったら終了
		while(formula.IndexOf("*") != -1){
			//演算子区切りで分割
			split = formula.Replace("+",",+,");
			split = split.Replace("~",",~,");
			split = split.Replace("*",",*,");
			split = split.Replace("/",",/,");
			//区切りを配列化
			string[] formula_data = split.Split(',');

			if(formula_data[i] == "*"){
				formula_data[i] = (double.Parse(getvalue(formula_data[i-1])) * double.Parse(getvalue(formula_data[i+1]))).ToString();
				formula_data[i-1] = null;
				formula_data[i+1] = null;
				formula = bond(formula_data);
				i = 0;
			}
			i++;
		}

		i = 0;
		//足し算がなくなったら=1つの数字になったら終了
		while(formula.IndexOf("+") != -1){
			//演算子区切りで分割
			split = formula.Replace("+",",+,");
			split = split.Replace("~",",~,");
			split = split.Replace("*",",*,");
			split = split.Replace("/",",/,");
			//区切りを配列化
			string[] formula_data = split.Split(',');

			if(formula_data[i] == "+"){
				formula_data[i] = (double.Parse(getvalue(formula_data[i-1])) + double.Parse(getvalue(formula_data[i+1]))).ToString();
				formula_data[i-1] = null;
				formula_data[i+1] = null;
				formula = bond(formula_data);
				i = 0;
			}
			i++;
		}

		i = 0;
		//引き算がなくなったら=1つの数字になったら終了
		while(formula.IndexOf("~") != -1){
			//演算子区切りで分割
			split = formula.Replace("+",",+,");
			split = split.Replace("~",",~,");
			split = split.Replace("*",",*,");
			split = split.Replace("/",",/,");
			//区切りを配列化
			string[] formula_data = split.Split(',');

			if(formula_data[i] == "~"){
				formula_data[i] = (double.Parse(getvalue(formula_data[i-1])) - double.Parse(getvalue(formula_data[i+1]))).ToString();
				formula_data[i-1] = null;
				formula_data[i+1] = null;
				formula = bond(formula_data);
				i = 0;
			}
			i++;
		}

		return double.Parse(formula);
	}

	string bond(string[] formula_data){
		//配列を1つの文字列に結合する関数

		string formula = "";
		foreach(string data in formula_data){
			if(data != null){
				formula += data;
			}
		}
		return formula;
	}

	string strsolve(string formula){
		//文字列結合関数

		String split;
		int i = 0;

		//足し算がなくなったら=1つの数字になったら終了
		while(formula.IndexOf("+") != -1){
			//演算子区切りで分割
			split = formula.Replace("+",",+,");
			//区切りを配列化
			string[] formula_data = split.Split(',');

			if(formula_data[i] == "+"){
				formula_data[i] = getvalue(formula_data[i-1]) + getvalue(formula_data[i+1]);
				formula_data[i-1] = null;
				formula_data[i+1] = null;
				formula = bond(formula_data);
				i = 0;
			}
			i++;
		}
		return formula;
	}
}
