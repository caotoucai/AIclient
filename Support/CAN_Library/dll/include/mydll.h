#ifndef MYDLL_H
#define MYDLL_H

#include "MyDLL_global.h"
#include <QString>

class MYDLL_EXPORT MyDLL
{
public:
    MyDLL();
    int add(int a,int b);
    int show(const char* name);
    QString m_s;
};

#endif // MYDLL_H
