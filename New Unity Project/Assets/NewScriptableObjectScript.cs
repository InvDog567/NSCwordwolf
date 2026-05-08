using UnityEngine;
using System;
using System.Collections.Generic; // ช่วยให้ใช้ List เพื่อเพิ่มข้อมูลได้เรื่อยๆ อย่างง่ายดาย

// บรรทัดนี้จะทำให้คุณคลิกขวาใน Unity แล้วเลือก Create > Game Data > Story System เพื่อสร้างไฟล์ได้
[CreateAssetMenu(fileName = "NewStoryData", menuName = "Game Data/Story System")]
public class StoryData : ScriptableObject
{
    // นี่คือรายการของภาษาต่างๆ (เช่น ไทย, อังกฤษ, ญี่ปุ่น)
    public List<LanguageContent> languages = new List<LanguageContent>();
}

[Serializable] // บรรทัดนี้สำคัญมาก! ทำให้ข้อมูลไปโชว์ในหน้าจอ Unity ให้เราพิมพ์ได้
public class LanguageContent
{
    public string languageName; // ชื่อภาษา
    public List<DialogueLine> dialogueLines = new List<DialogueLine>(); // รายการบทสนทนาในภาษานี้
}

[Serializable]
public class DialogueLine
{
    public Sprite characterIcon; // ช่องลากรูปตัวละครใส่
    public string speakerName;   // ช่องพิมพ์ชื่อคนพูด

    [TextArea(3, 5)]             // ทำให้ช่องพิมพ์ข้อความใหญ่ขึ้น พิมพ์ง่าย
    public string message;       // ช่องพิมพ์บทพูด
}