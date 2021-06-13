from tkinter import *
import time
import random
import math


# Trieda pre zoznam najblizsich bodov pre potreby
# najdenia najblizsich susedov. Implementovana
# spajanym zoznamom bodov (len ich vzdialenost
# a farba), usporiadanych podla vzdialenosti od
# bodu, ktory klasifikujeme.
class NeighborList():
    def __init__(self, neighborCount):
        self.neighborCount = neighborCount
        self.head = None
        self.currentCount = 0

    # Metoda na insert do spajaneho zoznamu,
    # po ktorom ostane zoznam usporiadany
    # podla vzdialenosti.
    def add_neighbor(self, distance, color):
        # Specialny pripad, ze zoznam je prazdny
        if (self.head == None):
            self.head = Node(distance, color)
            self.currentCount += 1
            return

        # Specialny pripad, ze novy prvok je blizsie nez prvy v zozname
        if (distance < self.head.distance):
            newNode = Node(distance, color)
            newNode.next = self.head
            self.head = newNode
            self.currentCount += 1
            return

        # Pridanie na ine miesto v zozname
        xNode = self.head
        x = 1
        while (xNode.next != None):
            if (x == self.neighborCount):   # Ak sme nenasli miesto medzi prvymi N prvkami, kde
                xNode.next = None           # N je pocet hladanych susedov, dalej uz nehladame.
                return

            if (distance < xNode.next.distance):
                newNode = Node(distance, color)
                newNode.next = xNode.next
                xNode.next = newNode
                self.currentCount += 1
                return

            xNode = xNode.next
            x += 1

        xNode.next = Node(distance, color)
        self.currentCount += 1

# Trieda jedneho vrcholu zoznamu (mohla by byt
# nahradena tupple, ale takyto objekt je mutable).
class Node():
    def __init__(self, distance, color):
        self.distance = distance
        self.color = color
        self.next = None


# Hlavna trieda klasifikatora, ktory sme mali naprogramovat.
class Klasifikator():
    # Pri vytvoreni zadame pocet bodov, ktore budeme generovat a klasifikovat.
    def __init__(self, pointCount):
        self.pointCount = pointCount
        self.points = None
        self.neighborCount = None
        self.count = None
        self.sum = None

        #Slovnik na rychle vyhodnotenie, ci som vygeneroval bod s unikatnymi suradnicami
        self.pointDict = {"-4500-4400": True,
                          "-4100-3000": True,
                          "-1800-2400": True,
                          "-2500-3400": True,
                          "-2000-1400": True,
                          "4500-4400":  True,
                          "4100-3000":  True,
                          "1800-2400":  True,
                          "2500-3400":  True,
                          "2000-1400":  True,
                          "-45004400":  True,
                          "-41003000":  True,
                          "-18002400":  True,
                          "-25003400":  True,
                          "-20001400":  True,
                          "45004400":   True,
                          "41003000":   True,
                          "18002400":   True,
                          "25003400":   True,
                          "20001400":   True}

        # Vsetky body generujeme iba raz a kazda klasifikacia potom
        # pracuje s tym istym vstupom. Body generujeme 4 naraz (1 z
        # kazdej triedy: cerveny, zeleny, modry, fialovy)
        self.generatedPoints = []
        for i in range(int(pointCount/4)):
            #Cervene
            if (random.randrange(100) + 1 != 1):                    # 99% sanca, ze bod bude vygenerovany zo
                x = random.randrange(-5000, 500)                    # zadaneho rozsahu pre konkretnu farbu.
                y = random.randrange(-5000, 500)
                while (self.pointDict.get(str(x) + str(y)) != None):# Body su generovane unikatne.
                    x = random.randrange(-5000, 500)
                    y = random.randrange(-5000, 500)
                self.pointDict[str(x) + str(y)] = True
                self.generatedPoints.append((x, y))
            else:                                                   # 1% sanca, ze bod bude vygenerovany nahodne.
                x = random.randrange(-5000, 5001)
                y = random.randrange(-5000, 5001)
                while (self.pointDict.get(str(x) + str(y)) != None):# -||-
                    x = random.randrange(-5000, 5001)
                    y = random.randrange(-5000, 5001)
                self.pointDict[str(x) + str(y)] = True
                self.generatedPoints.append((x, y))

            #Zelene
            if (random.randrange(100) + 1 != 1):                    # -||-
                x = random.randrange(-499, 5001)
                y = random.randrange(-5000, 500)
                while (self.pointDict.get(str(x) + str(y)) != None):# -||-
                    x = random.randrange(-499, 5001)
                    y = random.randrange(-5000, 500)
                self.pointDict[str(x) + str(y)] = True
                self.generatedPoints.append((x, y))
            else:                                                   # -||-
                x = random.randrange(-5000, 5001)
                y = random.randrange(-5000, 5001)
                while (self.pointDict.get(str(x) + str(y)) != None):# -||-
                    x = random.randrange(-5000, 5001)
                    y = random.randrange(-5000, 5001)
                self.pointDict[str(x) + str(y)] = True
                self.generatedPoints.append((x, y))

            #Modre
            if (random.randrange(100) + 1 != 1):                    # -||-
                x = random.randrange(-5000, 500)
                y = random.randrange(-499, 5001)
                while (self.pointDict.get(str(x) + str(y)) != None):# -||-
                    x = random.randrange(-5000, 500)
                    y = random.randrange(-499, 5001)
                self.pointDict[str(x) + str(y)] = True
                self.generatedPoints.append((x, y))
            else:                                                   # -||-
                x = random.randrange(-5000, 5001)
                y = random.randrange(-5000, 5001)
                while (self.pointDict.get(str(x) + str(y)) != None):# -||-
                    x = random.randrange(-5000, 5001)
                    y = random.randrange(-5000, 5001)
                self.pointDict[str(x) + str(y)] = True
                self.generatedPoints.append((x, y))

            #Fialove
            if (random.randrange(100) + 1 != 1):                    # -||-
                x = random.randrange(-499, 5001)
                y = random.randrange(-499, 5001)
                while (self.pointDict.get(str(x) + str(y)) != None):# -||-
                    x = random.randrange(-499, 5001)
                    y = random.randrange(-499, 5001)
                self.generatedPoints.append((x, y))
            else:                                                   # -||-
                x = random.randrange(-5000, 5001)
                y = random.randrange(-5000, 5001)
                while (self.pointDict.get(str(x) + str(y)) != None):# -||-
                    x = random.randrange(-5000, 5001)
                    y = random.randrange(-5000, 5001)
                self.generatedPoints.append((x, y))

    # Metoda volana "pouzivatelom" triedy na klasifikovanie
    # vygenerovanych bodov (vzdy tych istych, pre porovnanie).
    # Parametrom je hodnota "k" k-NN algoritmu, teda pocet
    # najblizsich susedov, podla ktorych urcujeme triedu bodu.
    def classify_all(self, neighborCount):
        self.points = [None]*100            # Plocha 10000x10000 je organizovana do 100 svorcekov 1000x1000.
        self.count = 0
        self.sum = 0
        self.neighborCount = neighborCount

        # Na zaciatok do zoznamu uz klasifikovanych bodov, pridame zadane pociatocne body.
        self.add_point(-4500, -4400, "r")
        self.add_point(-4100, -3000, "r")
        self.add_point(-1800, -2400, "r")
        self.add_point(-2500, -3400, "r")
        self.add_point(-2000, -1400, "r")
        self.add_point( 4500, -4400, "g")
        self.add_point( 4100, -3000, "g")
        self.add_point( 1800, -2400, "g")
        self.add_point( 2500, -3400, "g")
        self.add_point( 2000, -1400, "g")
        self.add_point(-4500,  4400, "b")
        self.add_point(-4100,  3000, "b")
        self.add_point(-1800,  2400, "b")
        self.add_point(-2500,  3400, "b")
        self.add_point(-2000,  1400, "b")
        self.add_point( 4500,  4400, "p")
        self.add_point( 4100,  3000, "p")
        self.add_point( 1800,  2400, "p")
        self.add_point( 2500,  3400, "p")
        self.add_point( 2000,  1400, "p")


        start = time.time()
        # Body klasifikujeme po 4 naraz (rovnako ako boli generovane) a pridavame ich
        # medzi klasifikovane body, pricom vzdy ak sa bod klasifikuje rovnako ako bol
        # generovany pripocitavame 1 do suctu uspesnych klasifikacii, ktoru potom delime
        # celkovym poctom bodov, ktore sme generovali/klasifikovali a tak ziskame % uspesnosti.
        for i in range(0, self.pointCount, 4):
            #Cervene
            color = self.classify(self.generatedPoints[i][0], self.generatedPoints[i][1])
            if (color == "r"):
                self.sum += 1
            self.add_point(self.generatedPoints[i][0], self.generatedPoints[i][1], color)

            #Zelene
            color = self.classify(self.generatedPoints[i+1][0], self.generatedPoints[i+1][1])
            if (color == "g"):
                self.sum += 1
            self.add_point(self.generatedPoints[i+1][0], self.generatedPoints[i+1][1], color)

            #Modre
            color = self.classify(self.generatedPoints[i+2][0], self.generatedPoints[i+2][1])
            if (color == "b"):
                self.sum += 1
            self.add_point(self.generatedPoints[i+2][0], self.generatedPoints[i+2][1], color)

            #Fialove
            color = self.classify(self.generatedPoints[i+3][0], self.generatedPoints[i+3][1])
            if (color == "p"):
                self.sum += 1
            self.add_point(self.generatedPoints[i+3][0], self.generatedPoints[i+3][1], color)

        # Vysledok tejto klasifikacie zobrazime
        self.display(int((self.sum / self.pointCount) * 100), int(time.time() - start))

    # Metoda na pridanie klasifikovaneho bodu
    # medzi body ostatne klasifikovane body.
    def add_point(self, x1, y1, color):
        # Transformacia realnych suradnic
        # vygenerovaneho bodu na 1 zo 100
        # stvorcekov, do ktoreho bude bod
        # patrit.
        x2 = min(int((x1+5000)/1000), 9)
        y2 = min(int((y1+5000)/1000), 9)
        index = y2*10 + x2

        if (self.points[index] == None):
            self.points[index] = [(x1, y1, color)]
        else:
            self.points[index].append((x1, y1, color))

        self.count += 1

    # Pomocna metoda na vyhodnotenie, ci stvorcek
    # vzdialeny o dx a dy od stvorceka na nejakom
    # indexe existuje alebo nie.
    def exists(self, index, dx, dy):
        return (index % 10 + dx >= 0 and index % 10 + dx < 10) and \
               (int(index / 10) + dy >= 0 and int(index / 10) + dy < 10)

    # Metoda na klasifikaciu jedneho bodu (toto bolo nasou primarnou ulohou).
    # Navratovou hodnotou je "trieda" bodu, teda jeho farba.
    def classify(self, x, y):
        neighborList = NeighborList(self.neighborCount)
        if (self.count <= 3020):                # Kym sa pole uz klasifikovanych bodov aspon trochu nenaplni nema zmysel
            for i in range(100):                # robit nejaku optimalizaciu a prehladanie celeho pola trva kratko.
                if (self.points[i] != None):
                    for point in self.points[i]:
                        neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

        else:                                       # Ked uz je pole trochu zaplnene, cca 30 bodov na stvorcek, robim
            xRelative = min(int((x+5000)/1000), 9)  # optimalizaciu, ktora je zalozena na prehladavani len stvorcekov
            yRelative = min(int((y+5000)/1000), 9)  # v blizkosti stvorceka, do ktoreho bol bod, ktory klasifikujeme,
            index = yRelative*10 + xRelative        # vygenerovany. Takto sa vyhneme prehladavaniu velkeho poctu bodov,
                                                    # ktore nie su vobec blizko. Konkretny postup je popisany v dokumentacii.
            if (xRelative % 1000 < 500):
                if (yRelative % 1000 < 500):
                    if (self.exists(index, -1, -1)):
                        if (self.points[index-11] != None):
                            for point in self.points[index-11]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, 0, -1)):
                        if (self.points[index-10] != None):
                            for point in self.points[index-10]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, -1, 0)):
                        if (self.points[index-1] != None):
                            for point in self.points[index-1]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.points[index] != None):
                            for point in self.points[index]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                else:
                    if (self.exists(index, -1, 0)):
                        if (self.points[index-1] != None):
                            for point in self.points[index-1]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.points[index] != None):
                            for point in self.points[index]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, -1, 1)):
                        if (self.points[index+9] != None):
                            for point in self.points[index+9]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, 0, +1)):
                        if (self.points[index+10] != None):
                            for point in self.points[index+10]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

            else:
                if (yRelative % 1000 < 500):
                    if (self.exists(index, 0, -1)):
                        if (self.points[index-10] != None):
                            for point in self.points[index-10]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, 1, -1)):
                        if (self.points[index-9] != None):
                            for point in self.points[index-9]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.points[index] != None):
                            for point in self.points[index]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, 0, 1)):
                        if (self.points[index+1] != None):
                            for point in self.points[index+1]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                else:
                    if (self.points[index] != None):
                            for point in self.points[index]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, 0, 1)):
                        if (self.points[index+1] != None):
                            for point in self.points[index+1]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, 0, +1)):
                        if (self.points[index+10] != None):
                            for point in self.points[index+10]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

                    if (self.exists(index, 1, 1)):
                        if (self.points[index+11] != None):
                            for point in self.points[index+11]:
                                neighborList.add_neighbor(math.sqrt((point[0] - x) ** 2 + (point[1] - y) ** 2), point[2])

        return self.get_color(neighborList.head)

    # Metoda na urcenie farby zo zoznamu susedov.
    def get_color(self, head):
        xNode = head
        red = []
        green = []
        blue = []
        purple = []
        x = 0

        # Prejdem zoznam susedov a rozdelim ho na jednotlive farby.
        while (xNode != None):
            if (x == self.neighborCount):
                break
            if (xNode.color == "r"):
                red.append(xNode.distance)
            if (xNode.color == "g"):
                green.append(xNode.distance)
            if (xNode.color == "b"):
                blue.append(xNode.distance)
            if (xNode.color == "p"):
                purple.append(xNode.distance)

            xNode = xNode.next
            x += 1

        maxLength = max(len(red), len(green), len(blue), len(purple))
        maxColor = None

        # Vyberieme farbu, ktorej bolo medzi susedmi najviac.
        # V pripade, ze na prvom mieste su dve farby s rovnakym
        # poctom bodov, vyberieme farbu, ktorej celkovy sucet
        # vzdialenosti bodov medzi susedmi je mensi.
        if (len(red) == maxLength):
            total = 0
            for distance in red:
                total += distance
            maxColor = ("r", total)

        if (len(green) == maxLength):
            total = 0
            for distance in green:
                total += distance

            if (maxColor != None):

                if (total < maxColor[1]):
                    maxColor = ("g", total)
            else:
                maxColor = ("g", total)

        if (len(blue) == maxLength):
            total = 0
            for distance in blue:
                total += distance

            if (maxColor != None):

                if (total < maxColor[1]):
                    maxColor = ("b", total)
            else:
                maxColor = ("b", total)

        if (len(purple) == maxLength):
            total = 0
            for distance in purple:
                total += distance

            if (maxColor != None):

                if (total < maxColor[1]):
                    maxColor = ("p", total)
            else:
                maxColor = ("p", total)

        return maxColor[0]

        #if (len(red) == maxLength):                        # Alternativny sposob vyberu, kde ak su dve farby
        #    maxColor = ("r", red[0])                       # na prvom mieste (z hladiska poctu), vyberiem z
                                                            # nich tu, prvy bod ktorej je blizsie.
        #if (len(green) == maxLength):
        #    if (maxColor != None):
        #        if (green[0] < maxColor[1]):
        #            maxColor = ("g", green[0])
        #    else:
        #        maxColor = ("g", green[0])

        #if (len(blue) == maxLength):
        #    if (maxColor != None):
        #        if (blue[0] < maxColor[1]):
        #            maxColor = ("b", blue[0])
        #    else:
        #        maxColor = ("b", blue[0])

        #if (len(purple) == maxLength):
        #    if (maxColor != None):
        #        if (purple[0] < maxColor[1]):
        #            maxColor = ("p", purple[0])
        #    else:
        #        maxColor = ("p", purple[0])

    # Metoda na zobrazenie vysledku klasifikacie,
    # vratane casu a percentualnej uspesnosti.
    def display(self, success, time):
        result = Tk()
        result.wm_title(str(self.neighborCount) + "-NN: " + str(success) + "% (" + str(time) + "s)")
        canvas = Canvas(result, height=1000, width=1000)
        canvas.pack()

        for i in range(100):
            if (self.points[i] != None):
                for point in self.points[i]:
                    x = int((point[0] + 5000) / 10)
                    y = int((point[1] + 5000) / 10)
                    if (point[2] == "r"):
                        color = "red"
                    if (point[2] == "g"):
                        color = "green"
                    if (point[2] == "b"):
                        color = "blue"
                    if (point[2] == "p"):
                        color = "purple"

                    canvas.create_oval(x-7, y-7, x+7, y+7, fill=color, outline='')

        result.mainloop()



c = Klasifikator(int(input("Pocet vygenerovanych bodov: ")))
while (True):
    c.classify_all(int(input("?-NN: ")))
