using UnityEngine;
using System.Collections.Generic;

 public class Leaf
{
    public RectInt area;
    public Leaf left, right;
    public RectInt? room;

    private int minLeafSize = 20;

    public Leaf(RectInt area)
    {
        this.area = area;
    }

    public bool Split()
    {
        bool splitHorizontally = Random.value > 0.5f;

        if (area.width > area.height && area.width / area.height >= 1.25f)
            splitHorizontally = false;
        else if (area.height > area.width && area.height / area.width >= 1.25f)
            splitHorizontally = true;

        int max = (splitHorizontally ? area.height : area.width) - minLeafSize;
        if (max <= minLeafSize) return false;

        int split = Random.Range(minLeafSize, max);

        if (splitHorizontally)
        {
            left = new Leaf(new RectInt(area.x, area.y, area.width, split));
            right = new Leaf(new RectInt(area.x, area.y + split, area.width, area.height - split));
        }
        else
        {
            left = new Leaf(new RectInt(area.x, area.y, split, area.height));
            right = new Leaf(new RectInt(area.x + split, area.y, area.width - split, area.height));
        }

        return true;
    }

    public void CreateRoom()
    {
        if (left != null || right != null)
        {
            left?.CreateRoom();
            right?.CreateRoom();
        }
        else
        {
            int roomWidth = Random.Range(10, area.width - 2);
            int roomHeight = Random.Range(6, area.height - 2);
            int roomX = area.x + Random.Range(1, area.width - roomWidth - 1);
            int roomY = area.y + Random.Range(1, area.height - roomHeight - 1);
            room = new RectInt(roomX, roomY, roomWidth, roomHeight);
        }
    }

    public List<RectInt> GetRooms()
    {
        List<RectInt> result = new();
        if (room != null)
        {
            result.Add(room.Value);
        }
        else
        {
            if (left != null) result.AddRange(left.GetRooms());
            if (right != null) result.AddRange(right.GetRooms());
        }
        return result;
    }
}
