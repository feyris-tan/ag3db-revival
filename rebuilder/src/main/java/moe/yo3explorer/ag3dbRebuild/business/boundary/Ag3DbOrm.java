package moe.yo3explorer.ag3dbRebuild.business.boundary;

import moe.yo3explorer.ag3dbRebuild.business.entity.DbCharacter;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.jetbrains.annotations.NotNull;

import java.io.Closeable;
import java.io.IOException;
import java.io.InputStream;
import java.sql.*;
import java.util.Properties;

public class Ag3DbOrm implements Closeable
{
    public Ag3DbOrm() throws IOException, SQLException {
        InputStream resourceAsStream = getClass().getClassLoader().getResourceAsStream("sql.properties");
        Properties properties = new Properties();
        properties.load(resourceAsStream);
        resourceAsStream.close();

        connection = DriverManager.getConnection(properties.getProperty("url"),
                properties.getProperty("username"),
                properties.getProperty("password"));
        connection.setSchema("js3db");

        logger = LogManager.getLogger(getClass());
        logger.info("DB Connection ready");
    }

    private Logger logger;
    private Connection connection;

    private PreparedStatement stmt1;
    public DbCharacter findDbCharacterByCrc32(String crc32) throws SQLException {
        if (stmt1 == null)
            stmt1 = connection.prepareStatement("SELECT * FROM characters WHERE crc32 = ?");

        stmt1.setString(0,crc32);
        ResultSet resultSet = stmt1.executeQuery();
        DbCharacter result = null;
        if (resultSet.next())
        {
            result = readCharacterResultSet(resultSet);
        }
        if (resultSet.next())
        {
            resultSet.close();
            return null;
        }
        resultSet.close();
        return result;
    }

    private @NotNull DbCharacter readCharacterResultSet(@NotNull ResultSet rs) throws SQLException {
        DbCharacter result = new DbCharacter();
        result.id = rs.getInt(1);
        result.type_id = rs.getInt(2);
        result.set_id = rs.getInt(3);
        result.user_id = rs.getInt(4);
        result.name = rs.getString(5);
        result.link = rs.getString(6);
        result.crc32 = rs.getString(7);
        result.size = rs.getInt(8);
        result.time_added = rs.getTimestamp(9);
        result.description = rs.getString(10);
        result.tags = rs.getString(11);
        result.hits = rs.getInt(12);
        result.rating = rs.getDouble(13);
        result.disabled = rs.getBoolean(14);
        return result;
    }

    private PreparedStatement stmt2;
    public DbCharacter findFirstRowCharacter() throws SQLException {
        if (stmt2 == null)
            stmt2 = connection.prepareStatement("SELECT * FroM characters LIMIT 1");

        ResultSet resultSet = stmt2.executeQuery();
        resultSet.next();
        DbCharacter result = readCharacterResultSet(resultSet);
        resultSet.close();
        return result;
    }

    @Override
    public void close() throws IOException {
        try {
            connection.close();
        } catch (SQLException throwables) {
        }
    }
}
