#include "FXRevisedElementType.h"

#include <QDomElement>
#include <QDebug>

namespace ASAM {
namespace FIBEX {

FXRevisedElementType::FXRevisedElementType() :
    FXNamedElementType()
{
}

void FXRevisedElementType::load(const QDomElement &element)
{
    FXNamedElementType::load(element);
}

}
}
