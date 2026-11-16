import bpy, math, os, random
from mathutils import Vector

# Pokemon environment pack | Blender 5.x
# Creates each asset in its own collection and exports one FBX per collection.
# Axes: Z up, model front -Y. Bushes do NOT contain fruit.
EXPORT_FBX = True
EXPORT_DIR = os.path.join(os.path.expanduser("~"), "PokemonEnvironmentExports")
random.seed(17)

def mat(name, color):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    m.node_tree.nodes["Principled BSDF"].inputs["Base Color"].default_value = (*color, 1)
    m.node_tree.nodes["Principled BSDF"].inputs["Roughness"].default_value = 0.82
    return m

M = {
 "leaf1":mat("ENV_Leaf_Dark",(0.07,.25,.06)), "leaf2":mat("ENV_Leaf_Mid",(.13,.43,.08)),
 "leaf3":mat("ENV_Leaf_Light",(.30,.60,.13)), "wood":mat("ENV_Wood",(.32,.17,.08)),
 "wood2":mat("ENV_Wood_Light",(.52,.32,.15)), "darkwood":mat("ENV_Wood_Dark",(.12,.055,.025)),
 "grass1":mat("ENV_Grass_Dark",(.10,.30,.06)), "grass2":mat("ENV_Grass_Mid",(.22,.48,.09)),
 "grass3":mat("ENV_Grass_Light",(.43,.65,.16)), "rock":mat("ENV_Rock",(.34,.37,.35)),
 "rocklight":mat("ENV_Rock_Light",(.51,.52,.47)), "rockdark":mat("ENV_Rock_Dark",(.20,.23,.22)),
 "red":mat("ENV_Fruit_Red",(.80,.025,.045)), "redhi":mat("ENV_Fruit_Red_Highlight",(1,.15,.12)),
 "blue":mat("ENV_Fruit_Blue",(.035,.18,.76)), "bluehi":mat("ENV_Fruit_Blue_Highlight",(.17,.43,1)),
 "stem":mat("ENV_Fruit_Stem",(.17,.31,.055))
}
ROOT=bpy.context.scene.collection
COL=None

def begin(name):
    global COL
    COL=bpy.data.collections.new(name); ROOT.children.link(COL)

def put(obj,name,ma=None,smooth=True):
    obj.name=name
    for c in list(obj.users_collection): c.objects.unlink(obj)
    COL.objects.link(obj)
    if ma: obj.data.materials.append(ma)
    if obj.type=="MESH" and smooth:
        for f in obj.data.polygons: f.use_smooth=True
    return obj

def sphere(name,loc,scale,ma,seg=16,rings=12):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,location=loc)
    o=bpy.context.object; o.scale=scale
    return put(o,name,ma)

def ico(name,loc,scale,ma):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2,radius=1,location=loc)
    o=bpy.context.object; o.scale=scale
    return put(o,name,ma,False)

def cyl(name,loc,radius,depth,ma,verts=12,rotation=(0,0,0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=radius,depth=depth,location=loc,rotation=rotation)
    return put(bpy.context.object,name,ma,False)

def cone_between(name,a,b,r1,r2,ma,verts=10):
    a,b=Vector(a),Vector(b); d=b-a
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r1,radius2=r2,depth=d.length,location=(a+b)/2)
    o=bpy.context.object; o.rotation_mode="QUATERNION"; o.rotation_quaternion=d.to_track_quat("Z","Y")
    return put(o,name,ma)

def blade(name,a,b,width,ma):
    a,b=Vector(a),Vector(b); d=(b-a).normalized()
    side=d.cross(Vector((0,1,0)))
    if side.length<.001: side=d.cross(Vector((1,0,0)))
    side.normalize(); mid=a.lerp(b,.48)+Vector((0,0,.035))
    verts=[a,mid-side*width/2,mid+Vector((0,0,.035)),mid+side*width/2,b]
    mesh=bpy.data.meshes.new(name+"_Mesh"); mesh.from_pydata(verts,[],[(0,1,2),(0,2,3),(1,4,2),(2,4,3)])
    mesh.materials.append(ma); mesh.update()
    o=bpy.data.objects.new(name,mesh); COL.objects.link(o)
    sol=o.modifiers.new("Leaf_Thickness","SOLIDIFY"); sol.thickness=.018
    bev=o.modifiers.new("Soft_Edges","BEVEL"); bev.width=.01; bev.segments=2
    return o

def export_end(name):
    global COL
    if EXPORT_FBX:
        os.makedirs(EXPORT_DIR,exist_ok=True)
        bpy.ops.object.select_all(action="DESELECT")
        objs=list(COL.all_objects)
        for o in objs:
            if o.type in {"MESH","EMPTY","ARMATURE"}: o.select_set(True)
        if objs:
            bpy.context.view_layer.objects.active=objs[0]
            bpy.ops.export_scene.fbx(filepath=os.path.join(EXPORT_DIR,name+".fbx"),
                use_selection=True,apply_unit_scale=True,axis_forward="-Z",axis_up="Y",
                object_types={"MESH","EMPTY","ARMATURE"})
            print("FBX:",os.path.join(EXPORT_DIR,name+".fbx"))
    COL=None

# 1. Two bush styles. Keep both berry-free so fruits can be attached in Unity.
def bush_round():
    begin("Bush_Round")
    sphere("Bush_Core",(0,0,.55),(.88,.72,.55),M["leaf1"],20,14)
    clusters=[(-.52,-.2,.57,.42),(-.2,-.4,.76,.47),(.28,-.35,.75,.48),(.58,-.05,.59,.42),
              (-.48,.3,.7,.42),(.02,.28,.9,.48),(.48,.27,.75,.42),(0,-.02,1.02,.43)]
    mats=[M["leaf2"],M["leaf3"],M["leaf1"]]
    for i,(x,y,z,r) in enumerate(clusters):
        sphere("Leaf_Cluster_%02d"%i,(x,y,z),(r,r*.88,r*.82),mats[i%3])
    for i,(a,b) in enumerate([
        ((-.5,0,.8),(-.83,-.02,1.15)),((.35,.12,1.0),(.65,.18,1.34)),
        ((-.05,-.2,1.12),(-.1,-.25,1.48)),((.58,-.03,.72),(.9,-.08,.98))]):
        blade("Leaf_Tip_%02d"%i,a,b,.28,M["leaf3"])
    export_end("Bush_Round")

def bush_branched():
    begin("Bush_Branched")
    ends=[(-.58,.02,1.25),(.52,.04,1.34),(-.18,.18,1.5),(.18,-.08,1.3),(.02,.25,1.15)]
    for i,e in enumerate(ends): cone_between("Branch_%02d"%i,(0,0,.12),e,.08,.03,M["wood"])
    pads=[((-.62,.02,1.18),(.42,.4,.34)),((.55,.05,1.26),(.47,.42,.37)),
          ((-.18,.15,1.45),(.43,.4,.37)),((.22,-.08,1.22),(.42,.4,.34)),
          ((.02,.24,1.1),(.46,.4,.32))]
    for i,(loc,sc) in enumerate(pads): sphere("Foliage_Pad_%02d"%i,loc,sc,[M["leaf1"],M["leaf2"],M["leaf3"]][i%3])
    export_end("Bush_Branched")

# 2. Separate red and blue fruit assets, origin at fruit centre.
def fruit(name,base,hi):
    begin(name)
    sphere("Fruit_Body",(0,0,0),(.22,.22,.25),base,20,16)
    sphere("Fruit_Highlight",(-.075,-.175,.085),(.045,.022,.065),hi,12,8)
    cone_between("Fruit_Stem",(0,0,.20),(.015,0,.31),.035,.02,M["stem"],8)
    for i in range(4):
        a=i*math.pi/2; x,y=math.cos(a)*.06,math.sin(a)*.06
        cone_between("Fruit_Sepal_%d"%i,(x,y,.205),(x*1.8,y*1.8,.235),.022,.003,M["stem"],6)
    export_end(name)

# 3. Three clumps of tall grass, individually exportable.
def grass(name,count,height,spread,seed):
    begin(name); rng=random.Random(seed); mats=[M["grass1"],M["grass2"],M["grass3"]]
    for i in range(count):
        a=rng.uniform(0,math.tau); r=rng.uniform(0,spread); x,y=math.cos(a)*r,math.sin(a)*r
        h=rng.uniform(.72,1.18)*height; lean=rng.uniform(.12,.38)
        tip=(x+math.cos(a)*lean+rng.uniform(-.15,.15),y+math.sin(a)*lean,h)
        blade("Grass_Blade_%02d"%i,(x,y,.03),tip,rng.uniform(.13,.22),mats[i%3])
    sphere("Grass_Base",(0,0,.09),(spread*.8,spread*.8,.12),M["grass1"])
    export_end(name)

# 4. Dead logs: solid and hollow.
def log_straight():
    begin("Log_Straight")
    cyl("Log_Main",(0,0,.42),.38,2.25,M["wood"],12,(math.pi/2,0,0))
    for y in (-1.14,1.14):
        cyl("Cut_End",(0,y,.42),.345,.035,M["wood2"],12,(math.pi/2,0,0))
    for y in (-.72,0,.72):
        bpy.ops.mesh.primitive_torus_add(major_radius=.375,minor_radius=.024,major_segments=12,minor_segments=6,
                                         location=(0,y,.42),rotation=(math.pi/2,0,0))
        put(bpy.context.object,"Bark_Ring",M["darkwood"])
    for i in range(4):
        a=i*math.tau/4
        sphere("Bark_Knot",(math.cos(a)*.34,-.5,.42+math.sin(a)*.3),(.08,.18,.07),M["darkwood"],12,8)
    export_end("Log_Straight")

def log_hollow():
    begin("Log_Hollow"); n=16; y0,y1=-1.15,1.15; ro,ri=.43,.27; verts=[]; faces=[]
    for y,r in [(y0,ro),(y1,ro),(y0,ri),(y1,ri)]:
        for i in range(n):
            a=math.tau*i/n; verts.append((math.cos(a)*r,y,.47+math.sin(a)*r))
    for i in range(n):
        j=(i+1)%n
        faces += [(i,j,n+j,n+i),(2*n+i,3*n+i,3*n+j,2*n+j),
                  (i,2*n+i,2*n+j,j),(n+i,n+j,3*n+j,3*n+i)]
    me=bpy.data.meshes.new("Hollow_Log_Mesh"); me.from_pydata(verts,[],faces)
    me.materials.append(M["wood"]); me.materials.append(M["darkwood"]); me.update()
    ob=bpy.data.objects.new("Hollow_Log_Tube",me); COL.objects.link(ob)
    for i,p in enumerate(me.polygons): p.material_index=1 if i%4==1 else 0
    cyl("Dark_Interior_Back",(0,1.02,.47),ri*.96,.025,M["darkwood"],16,(math.pi/2,0,0))
    for y in (-.72,-.1,.55): sphere("Bark_Patch",(0,y,.86),(.17,.25,.055),M["wood2"],12,8)
    export_end("Log_Hollow")

# 5. Medium and large trees, canopy clusters remain separate meshes.
def tree(name,height,cr):
    begin(name); th=height*.62; tr=.22 if height<4.5 else .30
    cyl("Tree_Trunk",(0,0,th/2),tr,th,M["wood"],10)
    for i in range(5):
        a=i*math.tau/5
        cone_between("Root_%02d"%i,(0,0,.5),(math.cos(a)*.48,math.sin(a)*.48,.1),tr*.55,.025,M["wood"],8)
    for i in range(4):
        a=i*math.tau/4+.35
        cone_between("Branch_%02d"%i,(0,0,th*.7),(math.cos(a)*cr*.62,math.sin(a)*cr*.62,th*.92),tr*.45,.045,M["wood"],8)
    cz=th+cr*.55
    clusters=[(0,0,cz+.35,.78),(-.62,-.1,cz,.68),(.62,-.08,cz+.04,.68),
              (-.28,.55,cz+.1,.68),(.35,.5,cz+.18,.67),(0,-.55,cz+.05,.7)]
    for i,(x,y,z,r) in enumerate(clusters):
        s=cr*r
        sphere("Canopy_Cluster_%02d"%i,(x*cr,y*cr,z),(s,s*.92,s*.86),
               [M["leaf1"],M["leaf2"],M["leaf3"]][i%3],18,12)
    export_end(name)

# 6. Five distinct standalone rocks.
def rock(name,scale,seed):
    begin(name); rng=random.Random(seed)
    ob=ico("Rock_Main",(0,0,scale[2]*.52),scale,M["rock"])
    for v in ob.data.vertices:
        v.co.x*=rng.uniform(.88,1.12); v.co.y*=rng.uniform(.88,1.12); v.co.z*=rng.uniform(.9,1.1)
    ob.data.update()
    for i in range(2):
        a=rng.uniform(0,math.tau)
        sphere("Rock_Facet_%02d"%i,(math.cos(a)*scale[0]*.48,math.sin(a)*scale[1]*.48,scale[2]*rng.uniform(.7,.92)),
               (scale[0]*.24,scale[1]*.18,scale[2]*.12),M["rocklight"] if i==0 else M["rockdark"],10,8)
    export_end(name)

# Remove only collections created by this script, preserving the rest of the scene.
for c in list(bpy.data.collections):
    if c.name.startswith(("Bush_","Fruit_","Grass_","Log_","Tree_","Rock_")):
        for o in list(c.objects): bpy.data.objects.remove(o,do_unlink=True)
        bpy.data.collections.remove(c)

bush_round()
bush_branched()
fruit("Fruit_Red",M["red"],M["redhi"])
fruit("Fruit_Blue",M["blue"],M["bluehi"])
grass("Grass_Clump_A",9,1.0,.25,11)
grass("Grass_Clump_B",13,1.25,.32,23)
grass("Grass_Clump_C",16,.82,.42,37)
log_straight()
log_hollow()
tree("Tree_Medium",3.8,1.15)
tree("Tree_Large",5.6,1.65)
rock("Rock_01",(.48,.38,.32),101)
rock("Rock_02",(.68,.52,.42),102)
rock("Rock_03",(.38,.34,.58),103)
rock("Rock_04",(.82,.55,.28),104)
rock("Rock_05",(.55,.62,.48),105)

print("Environment assets generated. Bushes have no fruit.")
print("FBX exports:",EXPORT_DIR)
