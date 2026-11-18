"""Reusable procedural geometry for the Ecoraki low-poly character."""
import math
import bpy
from mathutils import Vector

def make_material(name, color, roughness=0.78, emission=0.0):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = next((n for n in mat.node_tree.nodes if n.type == "BSDF_PRINCIPLED"), None)
    if bsdf is None:
        bsdf = mat.node_tree.nodes.new("ShaderNodeBsdfPrincipled")
        out = next((n for n in mat.node_tree.nodes if n.type == "OUTPUT_MATERIAL"), None)
        if out is None:
            out = mat.node_tree.nodes.new("ShaderNodeOutputMaterial")
        mat.node_tree.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Roughness"].default_value = roughness
    if emission and "Emission Color" in bsdf.inputs:
        bsdf.inputs["Emission Color"].default_value = color
        bsdf.inputs["Emission Strength"].default_value = emission
    return mat

def _mesh(name, verts, faces, materials, material_ids=None, smooth=True):
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.materials.clear()
    for m in materials:
        mesh.materials.append(m)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    if material_ids:
        for poly, idx in zip(mesh.polygons, material_ids):
            poly.material_index = idx
    for poly in mesh.polygons:
        poly.use_smooth = smooth
    return obj

def ellipsoid(name, center, radii, material, segments=24, rings=16, deform=None):
    cx, cy, cz = center
    rx, ry, rz = radii
    verts, faces = [], []
    for j in range(rings + 1):
        phi = math.pi * j / rings
        for i in range(segments):
            theta = 2 * math.pi * i / segments
            x = math.sin(phi) * math.cos(theta)
            y = math.sin(phi) * math.sin(theta)
            z = math.cos(phi)
            if deform:
                x, y, z = deform(x, y, z)
            verts.append((cx + rx*x, cy + ry*y, cz + rz*z))
    for j in range(rings):
        for i in range(segments):
            a=j*segments+i; b=j*segments+(i+1)%segments
            c=(j+1)*segments+(i+1)%segments; d=(j+1)*segments+i
            faces.append((a,b,c,d))
    return _mesh(name, verts, faces, [material])

def tube(name, points, radii, materials, sides=10, steps_per_span=5, closed=False):
    """Smooth variable-radius tube through Catmull-Rom control points."""
    pts=[Vector(p) for p in points]
    if len(pts)<2: raise ValueError("tube needs at least two control points")
    def sample(t):
        span=min(int(t),len(pts)-2); u=t-span
        p0=pts[max(0,span-1)]; p1=pts[span]; p2=pts[span+1]; p3=pts[min(len(pts)-1,span+2)]
        return 0.5*((2*p1)+(-p0+p2)*u+(2*p0-5*p1+4*p2-p3)*u*u+(-p0+3*p1-3*p2+p3)*u*u*u)
    count=(len(pts)-1)*steps_per_span+1
    centers=[sample((len(pts)-1)*i/(count-1)) for i in range(count)]
    def radius_at(i):
        t=(len(radii)-1)*i/(count-1)
        k=min(int(t),len(radii)-2); u=t-k
        return radii[k]*(1-u)+radii[k+1]*u
    verts=[]; faces=[]; mids=[]
    previous_side=None
    for i,c in enumerate(centers):
        tangent=(centers[min(i+1,count-1)]-centers[max(i-1,0)]).normalized()
        ref=Vector((0,0,1))
        if abs(tangent.dot(ref))>.92: ref=Vector((0,1,0))
        side=tangent.cross(ref).normalized()
        up=tangent.cross(side).normalized()
        r=radius_at(i)
        for s in range(sides):
            a=2*math.pi*s/sides
            v=c+r*(math.cos(a)*side+math.sin(a)*up)
            verts.append(tuple(v))
    for i in range(count-1):
        # Ring bands are material-defined from path distance.
        band = int((i/(count-1))*8) % len(materials)
        for s in range(sides):
            a=i*sides+s; b=i*sides+(s+1)%sides
            faces.append((a,b,b+sides,a+sides)); mids.append(band)
    faces.append(tuple(range(sides-1,-1,-1))); mids.append(0)
    last=(count-1)*sides
    faces.append(tuple(last+s for s in range(sides))); mids.append(0)
    return _mesh(name,verts,faces,materials,mids)

def leaf(name, base, tip, width, material, vein_material=None, segments=12):
    """Thick, cupped leaf blade with pointed tip and raised center ridge."""
    b=Vector(base); t=Vector(tip); axis=(t-b).normalized()
    side=axis.cross(Vector((0,1,0))).normalized()
    if side.length < .01: side=axis.cross(Vector((1,0,0))).normalized()
    normal=side.cross(axis).normalized()
    verts=[]; faces=[]
    # two surfaces, each with a centerline and paired margins
    for layer,sign in [(0,1),(1,-1)]:
        offset=normal*(.012*sign)
        for j in range(segments+1):
            u=j/segments
            w=width*(math.sin(math.pi*u)**0.82)
            center=b.lerp(t,u)+normal*(.045*math.sin(math.pi*u))
            for k in range(5):
                across=(k-2)/2
                p=center+side*w*across+offset
                p += normal*(.025*(1-abs(across))*math.sin(math.pi*u)*sign)
                verts.append(tuple(p))
    row=segments+1
    for layer in range(2):
        start=layer*row*5
        for j in range(segments):
            for k in range(4):
                a=start+j*5+k; faces.append((a,a+1,a+6,a+5) if layer==0 else (a+5,a+6,a+1,a))
    for j in range(segments):
        for k in (0,4):
            a=j*5+k; bidx=(j+1)*5+k
            faces.append((a,bidx,row*5+bidx,row*5+a))
    for k in range(4):
        faces.append((k,k+1,row*5+k+1,row*5+k))
        a=segments*5+k
        faces.append((a,row*5+a,row*5+a+1,a+1))
    obj=_mesh(name,verts,faces,[material],smooth=True)
    if vein_material:
        vein=tube(name+"_Vein",[base,b.lerp(t,.35)+normal*.04,b.lerp(t,.72)+normal*.025,tip],[.008,.006,.003,.001],[vein_material],sides=5,steps_per_span=3)
        return obj,vein
    return obj

def ear(name, center, height, width, outer, inner):
    """Pointed, slightly cupped lemur ear with a separate inset inner patch."""
    x,y,z=center
    verts=[]; faces=[]
    rings=14; sides=12
    for j in range(rings+1):
        u=j/rings
        w=width*(math.sin(math.pi*u)**.72)
        zz=z+height*u
        cy=y-.025*math.sin(math.pi*u)
        for i in range(sides):
            a=2*math.pi*i/sides
            verts.append((x+w*math.cos(a),cy+.045*math.sin(a),zz))
    for j in range(rings):
        for i in range(sides):
            a=j*sides+i;b=j*sides+(i+1)%sides
            faces.append((a,b,b+sides,a+sides))
    obj=_mesh(name,verts,faces,[outer],smooth=True)
    # Flattened inner ear leaf placed on forward (-Y) surface.
    patch=ellipsoid(name+"_Inner",(x,y-.043,z+height*.49),(width*.53,.025,height*.37),inner,16,10)
    return obj,patch

def curved_mark(name, points, material, radius=.012):
    obj=tube(name,points,[radius]*len(points),[material],sides=6,steps_per_span=4)
    return obj

def parent_to_collection(objects, collection):
    for obj in objects:
        for c in list(obj.users_collection):
            c.objects.unlink(obj)
        collection.objects.link(obj)
