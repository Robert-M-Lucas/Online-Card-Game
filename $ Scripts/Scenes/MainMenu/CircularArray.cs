using System.Collections.Generic;

public class CircularArray<T>{
    public T[] Array;

    int index = 0;
    public CircularArray(int Length) {
        Array = new T[Length];
    }

    public void Add(T item){
        Array[index] = item;
        index++;

        if (index >= Array.Length){
            index = 0;
        }
    }

    public int Contains(T item){
        for (int i = 0; i < Array.Length; i++){
            if (EqualityComparer<T>.Default.Equals(Array[i], item)){
                return i;
            }
        }

        return -1;
    }
}