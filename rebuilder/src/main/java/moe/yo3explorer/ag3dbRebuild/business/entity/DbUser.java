package moe.yo3explorer.ag3dbRebuild.business.entity;

import java.sql.Timestamp;

public class DbUser
{
    public int id;
    public Timestamp dateadded;
    public String username;
    public String salt;
    public String password;
    public String email;
}
