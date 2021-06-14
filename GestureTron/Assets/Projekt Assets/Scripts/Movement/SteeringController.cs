using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


public class SteeringController : MonoBehaviour
{
    [SerializeField]
    private string ip_address = "192.168.21.148";

    [SerializeField] private int device_port = 81;

    [SerializeField]
    private float pressureDivisor = 80;


    private Thread tcpThread;

    public int RightGlove;

    public int LeftGlove;

    public int Foot;

    private int[] footPressures = new int[15];

    private int fpIndex;
   /* [SerializeField]
    private int nullRange = 50;*/

    private TcpClient _client;
    private NetworkStream _stream;
    private Byte[] data;
    private int bytes;
    string responseData = string.Empty;

    private float rightActualPressure = 0f;
    private float leftActualPressure = 0f;
    private int footActualPressure = 0;

    public float RightActualPressure
    {
        get { return rightActualPressure/ pressureDivisor; }
    }
    public float LeftActualPressure
    {
        get { return leftActualPressure / pressureDivisor; }
    }
    public float FootActualPressure
    {
        get { return leftActualPressure / pressureDivisor; }
    }

    public float ActualPressure
    {
        get { return (rightActualPressure - leftActualPressure) / pressureDivisor; }
    }

    public float GetFootActualPressure()
    {
        return getAvarage(footPressures);
    }

    float getAvarage(int[] array)
    {
        float result = 0f;
        for (int i = 0; i < array.Length; i++)
        {
            result += array[i];
        }

        return result / array.Length;
    }
    void setupFootPressure(int actFootPress)
    {
        if (fpIndex > footPressures.Length - 1)
        {
            fpIndex = 0;
        }

        footPressures[fpIndex] = actFootPress;
        fpIndex++;
    }
    void connectToGlove(string server, int port)
    {
        _client = new TcpClient(server, port);
        _stream = _client.GetStream();
        _stream.ReadTimeout = 100;
        data = new Byte[125];
    }
    void splitData()
    {

        string[] tempData = responseData.Split(';');
        if (tempData.Length < 2)
            return;
        string[] tempValues = tempData[1].Split(',');

        if (tempValues[0] == RightGlove.ToString() 
            || tempValues[0] == LeftGlove.ToString() 
            ||tempValues[0] == Foot.ToString())
        {
            switch (tempValues[0])
            {
                case "16": //Láb
                    footActualPressure = int.Parse(tempValues[17]);
                    setupFootPressure(footActualPressure);
                    break;
                case "32": //Láb
                    footActualPressure = int.Parse(tempValues[17]);
                    setupFootPressure(footActualPressure);
                    break;
                case "48": //Baloldali L
                    leftActualPressure = float.Parse(tempValues[17]);
                    break;
                case "64": //Jobboldali XL
                    rightActualPressure = float.Parse(tempValues[17]);
                    break;
                case "80": //Baloldali XL
                    leftActualPressure = float.Parse(tempValues[17]);
                    break;
                case "96": //Jobboldali L
                    rightActualPressure = float.Parse(tempValues[17]);
                    break;
            }
        }
        //Debug.Log("Egéjsz:" + tempValues);
        //Debug.Log("Adat:" + tempValues[17]);
        //Debug.Log("leftPressure:"+ leftActualPressure);
        Debug.Log("rightPressure:" + rightActualPressure);


    }
    /*IEnumerator readData()
    {
        while (true)
        {
            if (_stream.CanRead && _stream.DataAvailable)
            {
                bytes = _stream.Read(data, 0, data.Length);
            }
            //try
            //{
            //    bytes = _stream.Read(data, 0, data.Length);
            //}
            //catch
            //{
            //    bytes = 0;
            //}

            //if (bytes == 0)
            //{
            //    yield return null;
            //}
            responseData = Encoding.Default.GetString(data, 0, bytes);
            string[] responseDatas;
            responseDatas = responseData.Split('\n');
            for (int i = 1; i < responseDatas.Length - 1; i++)
            {
                responseData = responseDatas[i];
                splitData();
            }

            yield return null;
        }
    }*/
    IEnumerator readData()
    {
        while (true)
        {
            int lbCnt = 0;
            int dcnt = 256;
            string r_msg = string.Empty;
            while (dcnt > 0)
            {
                if (_stream.CanRead && _stream.DataAvailable)
                {
                    char c = (char) _stream.ReadByte();
                    if (c == '\n' || c == '\r')
                    {
                        responseData = r_msg;
                        splitData();
                        r_msg = "";
                    }
                    else
                    {
                        r_msg += c;
                    }
                }

                dcnt--;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        connectToGlove(ip_address, device_port);
        StartCoroutine("readData");
        //tcpThread = new Thread(new ThreadStart(readData));
        /*BackgroundWorker backgroundWorker = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        backgroundWorker.DoWork += BackgroundWorkerOnDoWork;
        backgroundWorker.ProgressChanged += BackgroundWorkerOnProgressChanged;
        backgroundWorker.RunWorkerAsync();
        */
    }
    private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
    {
        string ip_address = "192.168.21.148";

        int device_port = 81;
        BackgroundWorker worker = (BackgroundWorker)sender;
        Byte[] data = new byte[500];
        int bytes;
        TcpClient _client = new TcpClient(ip_address, device_port);
        NetworkStream _stream = _client.GetStream();
        while (!worker.CancellationPending)
        {
            bytes = _stream.Read(data, 0, data.Length);
            string responseData = Encoding.Default.GetString(data, 0, bytes);
            string[] responseDatas;
            responseDatas = responseData.Split('\n');
            for (int i = 1; i < responseDatas.Length - 1; i++)
            {
                responseData = responseDatas[i];
                string[] tempData = responseData.Split(';');
                string[] tempValues = tempData[1].Split(',');
                if (tempValues[0] == "96")
                {
                    float rrightActualPressure = float.Parse(tempValues[17]);
                    worker.ReportProgress(0, rrightActualPressure);
                }
                
                
                //splitData();
            }
           ;
        }
    }
    private void BackgroundWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        rightActualPressure = float.Parse(e.UserState.ToString());
        Debug.Log("Ide beléptem more: " + rightActualPressure);
        int percentage = e.ProgressPercentage;
    }


}

