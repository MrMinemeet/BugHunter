<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.2" tiledversion="1.2.1" name="collision" tilewidth="64" tileheight="64" tilecount="9" columns="3">
 <image source="tiles/trigger_collision.png" trans="ff00ff" width="192" height="192"/>
 <tile id="0" type="Collision">
  <properties>
   <property name="HasCollision" type="bool" value="true"/>
  </properties>
 </tile>
 <tile id="1" type="EnemySpawn"/>
 <tile id="2" type="Reload"/>
 <tile id="3" type="PlayerSpawn"/>
</tileset>
