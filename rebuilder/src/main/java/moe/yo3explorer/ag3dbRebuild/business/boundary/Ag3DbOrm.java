package moe.yo3explorer.ag3dbRebuild.business.boundary;

import moe.yo3explorer.ag3dbRebuild.business.entity.DbCharacter;
import moe.yo3explorer.ag3dbRebuild.business.entity.DbUser;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedCharacter;
import moe.yo3explorer.ag3dbRebuild.business.entity.ExtractedRating;
import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.jetbrains.annotations.NotNull;

import java.io.Closeable;
import java.io.IOException;
import java.io.InputStream;
import java.math.BigInteger;
import java.sql.*;
import java.util.Properties;
import java.util.Random;

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
        connection.setAutoCommit(false);

        random = new Random();
        logger = LogManager.getLogger(getClass());
        logger.info("DB Connection ready");
    }

    private Logger logger;
    private Connection connection;
    private Random random;

    private PreparedStatement stmt1;
    public DbCharacter findDbCharacterByCrc32(String crc32) throws SQLException {
        if (stmt1 == null)
            stmt1 = connection.prepareStatement("SELECT * FROM characters WHERE crc32 = ?");

        stmt1.setString(1,crc32);
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

    private PreparedStatement stmt3;
    public boolean testForCharacterData(int id) throws SQLException {
        if (stmt3 == null)
            stmt3 = connection.prepareStatement("SELECT dateadded FROM character_data WHERE char_id=?");

        stmt3.setInt(1,id);
        ResultSet resultSet = stmt3.executeQuery();
        boolean result = resultSet.next();
        resultSet.close();
        return result;
    }

    public DbUser autoGetDbUser(@NotNull DbCharacter db, UserParseService extracted) throws SQLException {
        DbUser userById = getUserById(db.user_id);
        if (userById != null)
            return userById;

        DbUser userByName = getUserByName(extracted.getUsername());
        if (userByName != null)
        {
            patchUserId(db,userByName.id);
            return userByName;
        }

        DbUser createThis = new DbUser();
        createThis.id = db.user_id;
        createThis.username = extracted.getUsername();
        createThis.salt = "";
        createThis.password = "";
        createThis.email = String.format("blackhole-%s@nowhere",new BigInteger(100,random).toString(26));
        createDbUser(createThis);
        return getUserById(db.user_id);
    }

    private PreparedStatement stmt4;
    private DbUser getUserById(int id) throws SQLException {
        if (stmt4 == null)
            stmt4 = connection.prepareStatement("SELECT * FROM users WHERE id=?");

        stmt4.setInt(1,id);
        ResultSet resultSet = stmt4.executeQuery();
        DbUser result = null;
        if (resultSet.next())
        {
            result = new DbUser();
            result.id = resultSet.getInt(1);
            result.dateadded = resultSet.getTimestamp(2);
            result.username = resultSet.getString(3);
            result.salt = resultSet.getString(4);
            result.password = resultSet.getString(5);
            result.email = resultSet.getString(6);
        }
        resultSet.close();
        return result;
    }

    private PreparedStatement stmt5;
    private void createDbUser(DbUser dbUser) throws SQLException {
        if (stmt5 == null)
            stmt5 = connection.prepareStatement(
                    "INSERT INTO users (id, username, salt, password, email) " +
                            "VALUES (?,?,?,?,?)");

        stmt5.setInt(1,dbUser.id);
        stmt5.setString(2,dbUser.username);
        stmt5.setString(3,dbUser.salt);
        stmt5.setString(4,dbUser.password);
        stmt5.setString(5,dbUser.email);
        stmt5.executeUpdate();
    }

    public void postCharacter(@NotNull ExtractedCharacter character, int id) throws SQLException {
        logger.info(String.format("Post character %s",character.basename));
        insertCharacterData(character,id);

        for (int i = 0; i < character.previews.length; i++)
        {
            if (character.previews[i] == null)
                break;
            insertPreview(id,i + 1,character.previews[i]);
        }

        for (ExtractedRating rating : character.ratings) {
            insertRating(rating);
        }

        connection.commit();
    }

    private PreparedStatement stmt6;
    private void insertCharacterData(ExtractedCharacter extractedCharacter, int id) throws SQLException {
        if (stmt6 == null)
            stmt6 = connection.prepareStatement("INSERT INTO character_data (char_id, js3cmi, sevenzip, v) VALUES (?,?,?,?)");

        stmt6.setInt(1,id);
        stmt6.setBytes(2,extractedCharacter.js3cmi);
        stmt6.setBytes(3,extractedCharacter.sevenzip);
        stmt6.setBytes(4,extractedCharacter.v);
        stmt6.executeUpdate();
    }

    private PreparedStatement stmt7;
    private void insertPreview(int charId, int ordinal, byte[] buffer) throws SQLException {
        if (stmt7 == null)
            stmt7 = connection.prepareStatement("INSERT INTO character_preview (char_id, ordinal, buffer) VALUES (?,?,?)");

        stmt7.setInt(1,charId);
        stmt7.setInt(2,ordinal);
        stmt7.setBytes(3,buffer);
        stmt7.executeUpdate();
    }

    private PreparedStatement stmt8;
    private void insertRating(ExtractedRating rating) throws SQLException {
        if (stmt8 == null)
            stmt8 = connection.prepareStatement("INSERT INTO character_ratings (rating, user_id, char_id) VALUES (?,?,?)");

        stmt8.setInt(1,rating.ratingValue);
        stmt8.setInt(2,rating.userId);
        stmt8.setInt(3,rating.characterId);
        stmt8.executeUpdate();
    }

    private PreparedStatement stmt9;
    private DbUser getUserByName(String username) throws SQLException {
        if (stmt9 == null)
            stmt9 = connection.prepareStatement("SELECT * FROM users WHERE username=?");

        stmt9.setString(1,username);
        ResultSet resultSet = stmt9.executeQuery();
        DbUser result = null;
        if (resultSet.next())
        {
            result = new DbUser();
            result.id = resultSet.getInt(1);
            result.dateadded = resultSet.getTimestamp(2);
            result.username = resultSet.getString(3);
            result.salt = resultSet.getString(4);
            result.password = resultSet.getString(5);
            result.email = resultSet.getString(6);
        }
        resultSet.close();
        return result;
    }

    private PreparedStatement stmt10;
    private void patchUserId(@NotNull DbCharacter dbCharacter, int newUserId) throws SQLException {
        logger.info(String.format("UPDATE character %d, set user_id = %d (was %d)",dbCharacter.id,newUserId,dbCharacter.user_id));
        if (stmt10 == null)
            stmt10 = connection.prepareStatement("UPDATE characters SET user_id = ? WHERE id = ?");

        stmt10.setInt(2,dbCharacter.id);
        stmt10.setInt(1,newUserId);
        int i = stmt10.executeUpdate();
        if (i != 1)
            throw new RuntimeException("what?");
    }
}
