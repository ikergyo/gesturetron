using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TrailCollider : MonoBehaviour
{
    public float time = 2.0f;
    public int rate = 10;

    public SetupLocalPlayer networkObject;

    Vector3[] arv3;
    //Head az utolsó érték, a legutóbbi pozció
    int head;
    //A vége, a legutolsó tárolt pozíció. Mivel a tömb méretétől függ ezért nem biztos hogy  a játék kezdete az első pozíció, hanem amennyi belefér az arv3 tömbbe.
    int tail = 0;
    //Egy szelet ideje, milyen időközönként vegyen fel új "mérést", vagyis mennyi legyen az eltelt idő két aktuális oizíció felvétele között. Minél kisebb annál sűrűbb.
    float sliceTime;
    //Ez az ütközésról ad infókat. Milyen colldier stb
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        sliceTime = 1.0f / rate;
        //Ez a tömb tárolja az eddigi poziíciókat
        arv3 = new Vector3[(Mathf.RoundToInt(time * rate) + 1)];

        for (var i = 0; i < arv3.Length; i++)
        {
            arv3[i] = transform.position;
        }
            
        head = arv3.Length - 1;
        /*
         * Meghívja a collectdata metódust. A coroutine ok unityben olyan függvények amik a yield től folytatják futáésok bizonyos idő elteltével.
         * Miután letetlt az idő a futás a yield től indul és akövi yield ig tart. Mivel ez egy végtelen ciklus, mindig futni fog. (A coroutine lehet hogy másik szálon fut de ezt nem tudom, ha nem érdees lehet másik szálra rakni a gyorsítás érdekében)
        */
        StartCoroutine(CollectData());
    }
    /*
     * Coroutinenként hívódik meg. megnézi hol tart a head, ha nem uyganaz mint a transform positoion épp akkor hozzá igazítja
     * Az osztások azért kellenek, hogy helyén legyen a head. 
     * Pl.: A tömb mérete 500. A head először 499 lesz a tail 0. Elmozdul a motor -> belemegy az ifbe. Head+ 1 = 500. 500%500 = 0
     * Tehát a 0ik helyen lesz a head(körbe ért), a tail pedig 1%500 = 1. tehát 1-> 499 ig miinden a head mögött van.
     * Kövi elmozdulásnál a head = 1, tail 2. head mögött minden ami 2 után van és a 0. Ez így megy végig, körbe körbe.
     */
    IEnumerator CollectData() {
        while (true) {
            if (transform.position != arv3[head]) {
                head = (head + 1) % arv3.Length;
                tail = (tail + 1) % arv3.Length;
                arv3[head] = transform.position;
            }
        yield return new WaitForSeconds(sliceTime);
    }
}

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Hit())
        {
           /* Debug.Log("I hit: " + hit.collider.name);
            Debug.Log("I hit: " + hit.collider.gameObject.transform.parent.gameObject.GetComponent<NetworkIdentity>().netId);*/
            //Ez a törléshez kell. Mindegyik kliensnél törlődik az object
            networkObject.Cmd_DestroyThis(hit.collider.gameObject.transform.parent.gameObject); 
        }
        

    }
    /*
     *  Végig megy a tömbbön. i a head, j  head -1. Ez azért kell mert megnézi, hog ya két érték között van egy collider. 
     *  Tehát ugye mintavételi időnként rak egy-egy mintát az arv3 tömbbe. Emiatt lehet ritkán, és sűrűn is. A linecast 
     *  egy olyan függvény ami megmondja, hogy két vektor(itt igazából inkább pont szerintem) van egy collider. Ha van akkor igazzal tér vissza.
     *  Lehet neki adni kimenetnek egy RaycastHit objektumot, abba beleírja a collider adatait. (Nem tudom miért out és miért nem &. Mondjuk lehet azért mert az osztályok alapból referencia típusúak de akkor is, nem imertem ezt az outot)
     *  
     *  TODO: Ez egy elég erőforrás igényes feladat, lehetne akár egy külön szálra küldeni, de ez elég hard a RaycastHit miatt, meg az update miatt.
     */
    bool Hit(){
        int i = head;
        int j = head - 1;
        if (j< 0) 
        {
            j = arv3.Length - 1;
        }
 
        while (j != head) {
            if (Physics.Linecast(arv3[i], arv3[j], out hit))
            {
                return true;
            }
            i = i - 1;
            if (i < 0)
            {
                i = arv3.Length - 1;
            }
            j = j - 1;
            if (j < 0)
            {
                j = arv3.Length - 1;
            }
        }
        return false;
    }
    
}
