package moe.yo3explorer.ag3dbRebuild.business.entity;

import java.sql.Timestamp;

public class DbCharacter
{
    public int id, type_id, set_id, user_id;
    public String name, link, crc32;
    public int size;
    public Timestamp time_added;
    public String description, tags;
    public int hits;
    public double rating;
    public boolean disabled;
}
